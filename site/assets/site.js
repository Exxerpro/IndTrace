// IndTrace project site: theme toggle, the hero line sequence, screenshot lightbox.
(function () {
    "use strict";

    // ---- theme ----------------------------------------------------------------------------------------
    var root = document.documentElement;
    var toggle = document.querySelector("[data-theme-toggle]");
    function stored() { try { return localStorage.getItem("theme"); } catch (e) { return null; } }
    function store(v) { try { localStorage.setItem("theme", v); } catch (e) { /* private mode */ } }
    var saved = stored();
    if (saved === "light" || saved === "dark") { root.setAttribute("data-theme", saved); }
    if (toggle) {
        toggle.addEventListener("click", function () {
            var dark = root.getAttribute("data-theme") === "dark" ||
                (!root.hasAttribute("data-theme") && window.matchMedia("(prefers-color-scheme: dark)").matches);
            var next = dark ? "light" : "dark";
            root.setAttribute("data-theme", next);
            store(next);
        });
    }

    // ---- the line -------------------------------------------------------------------------------------
    // One part travels its routing and every arrival is validated; a second part reports in at a station that
    // is not a legal successor and is recorded as a rejected arrival. The line itself is never commanded.
    var line = document.querySelector("[data-line]");
    var script = document.getElementById("line-script");
    if (line && script) {
        var steps = JSON.parse(script.textContent);
        var stations = line.querySelectorAll(".station");
        var reduce = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        function tag(label) {
            var el = document.createElement("span");
            el.className = "part-tag";
            el.setAttribute("aria-hidden", "true");
            var bars = document.createElement("span");
            bars.className = "bars";
            bars.textContent = label;
            el.appendChild(bars);
            return el;
        }

        function apply(step, animate) {
            var st = stations[step.station];
            if (!st) { return; }
            var slot = st.querySelector(".tag-slot");
            var verdict = st.querySelector(".verdict");
            line.querySelectorAll(".part-tag[data-part='" + step.part + "']").forEach(function (t) { t.remove(); });
            var t = tag(step.label);
            t.setAttribute("data-part", step.part);
            if (animate) { t.classList.add("moving"); }
            slot.appendChild(t);
            st.classList.remove("is-done", "is-reject");
            st.classList.add(step.ok ? "is-done" : "is-reject");
            verdict.className = "verdict " + (step.ok ? "ok" : "bad");
            verdict.textContent = step.text;
        }

        if (reduce || !("IntersectionObserver" in window)) {
            steps.forEach(function (s) { apply(s, false); });
        } else {
            var started = false;
            var io = new IntersectionObserver(function (entries) {
                if (started || !entries[0].isIntersecting) { return; }
                started = true;
                io.disconnect();
                steps.forEach(function (s, i) { setTimeout(function () { apply(s, true); }, 500 + i * 900); });
            }, { threshold: 0.4 });
            io.observe(line);
        }
    }

    // ---- lightbox -------------------------------------------------------------------------------------
    var box = document.querySelector("dialog.lightbox");
    if (box && typeof box.showModal === "function") {
        var big = box.querySelector("img");
        document.querySelectorAll("[data-zoom]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var img = btn.querySelector("img");
                big.src = btn.getAttribute("data-zoom");
                big.alt = img ? img.alt : "";
                box.showModal();
            });
        });
        box.addEventListener("click", function (e) { if (e.target === box) { box.close(); } });
    }
})();
