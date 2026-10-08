# Security policy

IndTrace runs next to production equipment, so we take vulnerability reports seriously.

## Reporting a vulnerability

**Do not open a public issue, discussion or pull request for a security problem.**

Report it privately through GitHub: on the repository's **Security** tab, choose **Report a vulnerability**
(GitHub private vulnerability reporting). If you cannot use GitHub, e-mail **exxerpro@exxerpro.com** with the
subject `SECURITY: IndTrace`.

Please include:

- the affected component (for example the gateway, the hub, the Monitor UI or Identity) and version or commit;
- the steps to reproduce, or a proof of concept;
- the impact you expect (data integrity, authentication bypass, denial of service of the gateway, and so on).

## What to expect

- We acknowledge the report within **5 business days**.
- We send an initial assessment within **15 business days**, and keep you informed until it is resolved.
- We coordinate the disclosure date with you, and credit you in the advisory unless you prefer otherwise.

## Supported versions

Only the latest commit on `main` receives security fixes.

## Scope

In scope: the source code in this repository. Out of scope: deployments operated by third parties, hardware
drivers that are not part of this repository, and findings that require physical access to the plant network
or an already compromised host.
