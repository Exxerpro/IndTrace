// <copyright file="ExpressionToKeyStringVisitor.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace IndTrace.Application.Repository;

using System.Text;

internal sealed class ExpressionToKeyStringVisitor : ExpressionVisitor
{
    private readonly StringBuilder builder = new();

    public static string ToKeyString(Expression expression)
    {
        var visitor = new ExpressionToKeyStringVisitor();
        visitor.Visit(expression);
        return visitor.builder.ToString();
    }

    /// <inheritdoc/>
    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        this.builder.Append($"{node.Parameters[0].Name} => ");
        this.Visit(node.Body);
        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitBinary(BinaryExpression node)
    {
        this.builder.Append("(");
        this.Visit(node.Left);
        this.builder.Append($" {node.NodeType} ");
        this.Visit(node.Right);
        this.builder.Append(")");
        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitMember(MemberExpression node)
    {
        // A member-access sub-tree that captures NO lambda parameter is fully closure-bound: evaluate it to
        // its RUNTIME VALUE so the cache key reflects the value, not a constant member name. This covers a
        // single-hop captured local (e.g. machineId), an N-hop closure chain (context.Request.PartNumber,
        // context.Machine.MachineId), and static member access. Collapsing distinct values to one key would
        // serve the wrong product/rule/references on the barcode-create path, so this must be exact.
        if (!ParameterDetector.Contains(node))
        {
            // Fast path: a direct captured local/field (single-hop closure) — reflect the value off the
            // closure instance without paying for expression compilation.
            if (node.Expression is ConstantExpression constExpr)
            {
                this.builder.Append(ValueToString(GetValueFromClosure(constExpr, node.Member)));
                return node;
            }

            // N-hop closure (context.Request.PartNumber) or static member: the sub-tree has no parameter, so
            // compile it as a parameterless lambda and invoke it to obtain the concrete runtime value.
            this.builder.Append(ValueToString(EvaluateFullyBound(node)));
            return node;
        }

        // Parameter-rooted member path: this is a queried column (e.g. m.MachineId or m.Machine.MachineType).
        // Render the full dotted path so distinct columns yield distinct keys.
        if (this.TryAppendParameterPath(node))
        {
            return node;
        }

        // Neither a fully closure-bound value nor a parameter-rooted member path (e.g. a member accessed on a
        // method-call result that still captures the parameter). Emitting a bare member name here would
        // collapse different runtime values onto one cache key — a poisoned key on a life-critical
        // traceability wire is never acceptable. This is a programmer error, so fail loud.
        throw new InvalidOperationException(
            $"ExpressionToKeyStringVisitor cannot render a stable cache key for member expression '{node}'. " +
            "The expression is neither a fully closure-bound value nor a parameter-rooted member path.");
    }

    /// <inheritdoc/>
    protected override Expression VisitConstant(ConstantExpression node)
    {
        this.builder.Append(ValueToString(node.Value));
        return node;
    }

    /// <inheritdoc/>
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Without this override the base visitor walks the object + arguments but emits NOTHING for the method
        // itself, so `someList.Contains(b.CycleId)` lost both its operation name AND (via the collection's
        // ToString) its list contents — collapsing distinct predicates onto one cache key. Emit the method
        // identity plus the (recursively visited) receiver and arguments so both the queried column
        // (b.CycleId, a parameter path) and the resolved collection contents ([1,2]) discriminate the key.
        if (node.Object is not null)
        {
            // Instance method: the receiver is either a closure-bound collection (resolved to its contents) or
            // a parameter path — Visit routes it correctly. No separate evaluation, so nothing is done twice.
            this.Visit(node.Object);
            this.builder.Append('.');
        }
        else
        {
            // Static / extension method (e.g. Enumerable.Contains(list, b.Col)): qualify with the declaring
            // type so different helpers with the same name do not collide.
            this.builder.Append(node.Method.DeclaringType?.Name ?? string.Empty).Append('.');
        }

        this.builder.Append(node.Method.Name).Append('(');
        for (var i = 0; i < node.Arguments.Count; i++)
        {
            if (i > 0)
            {
                this.builder.Append(',');
            }

            this.Visit(node.Arguments[i]);
        }

        this.builder.Append(')');
        return node;
    }

    private static object? GetValueFromClosure(ConstantExpression constantExpression, MemberInfo member)
    {
        return member switch
        {
            FieldInfo field => field.GetValue(constantExpression.Value),
            PropertyInfo prop => prop.GetValue(constantExpression.Value),
            _ => null,
        };
    }

    // Compiles a parameter-free sub-expression to a delegate and invokes it to obtain the concrete value.
    // Only ever called after ParameterDetector.Contains(node) returned false, so the lambda is genuinely
    // parameterless and DynamicInvoke needs no arguments.
    private static object? EvaluateFullyBound(Expression node)
    {
        var lambda = Expression.Lambda(node);
        var compiled = lambda.Compile();
        return compiled.DynamicInvoke();
    }

    // Renders a parameter-rooted member chain as a dotted path (e.g. "m.Machine.MachineType"). Returns false
    // when the chain does not bottom out in a ParameterExpression, so the caller can fail loud.
    private bool TryAppendParameterPath(MemberExpression node)
    {
        var names = new Stack<string>();
        Expression? current = node;
        while (current is MemberExpression member)
        {
            names.Push(member.Member.Name);
            current = member.Expression;
        }

        if (current is not ParameterExpression parameter)
        {
            return false;
        }

        this.builder.Append(parameter.Name);
        foreach (var name in names)
        {
            this.builder.Append('.').Append(name);
        }

        return true;
    }

    // Detects whether an expression sub-tree references any lambda parameter. A sub-tree with no parameter is
    // fully closure-bound and can be evaluated to a constant value; one that references a parameter is a
    // queried column path.
    private sealed class ParameterDetector : ExpressionVisitor
    {
        private bool found;

        public static bool Contains(Expression? expression)
        {
            if (expression is null)
            {
                return false;
            }

            var detector = new ParameterDetector();
            detector.Visit(expression);
            return detector.found;
        }

        /// <inheritdoc/>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            this.found = true;
            return node;
        }
    }

    private static string ValueToString(object? value)
    {
        return value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            DateTime dt => dt.ToString("O"), // ISO format

            // A non-string collection (the pervasive `someList.Contains(b.Column)` shape) MUST be rendered by
            // its CONTENTS, not ToString(). object.ToString() on a List<int>/array yields the content-INDEPENDENT
            // type name (e.g. "System.Collections.Generic.List`1[System.Int32]"), so two specs built from
            // different lists (cycleIds [1,2] vs [3,4]) would collapse onto ONE cache key and serve the first
            // caller's rows to the second. Recurse with the same value formatting so element values discriminate.
            System.Collections.IEnumerable enumerable => EnumerableToString(enumerable),

            _ => value.ToString() ?? string.Empty,
        };
    }

    // Renders a non-string enumerable as "[elem1,elem2,...]" using the same value formatting recursively, so the
    // element VALUES (not the collection's type name) land in the cache key. Full contents are emitted on
    // purpose: truncating would let two lists that share a long prefix collapse onto one poisoned key, which on
    // a life-critical traceability wire is never acceptable — correctness over key length.
    private static string EnumerableToString(System.Collections.IEnumerable enumerable)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        var first = true;
        foreach (var item in enumerable)
        {
            if (!first)
            {
                sb.Append(',');
            }

            first = false;
            sb.Append(ValueToString(item));
        }

        sb.Append(']');
        return sb.ToString();
    }
}

// Note: unresolvable member chains intentionally fail loud (InvalidOperationException) rather than emit a
// bare member name — a poisoned cache key on a life-critical traceability wire is never acceptable.