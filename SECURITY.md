# Security Policy

Security fixes target the current main application under WineApp/src/WineApp.
Legacy projects and old deployments are not maintained security targets.

## Reporting a Vulnerability

Do not post credentials, personal data, or exploit details in public issues.
Use GitHub's private vulnerability reporting option on the repository Security
page when it is enabled. If it is unavailable, ask the repository owner for a
private reporting channel without including vulnerability details publicly.

## Security Checks

Pull requests run unit tests, a direct/transitive NuGet advisory check, and
CodeQL C# analysis. Dependabot configuration requests weekly NuGet and GitHub
Actions updates. These files do not themselves enable hosted Dependabot alerts,
security updates, private reporting, or branch protection; a repository Admin
must verify those settings and re-enable workflows disabled for inactivity.

Advisory checks can lag newly disclosed issues. Review upstream release notes
and apply relevant security fixes even when an advisory query returns no matches.

## Access and Audit Policy

Judges can score only eligible wines in flights assigned to their immutable
user IDs. Non-Admin users cannot access results before explicit publication.
Registration and payment receipts are independent of result publication.
Audit history is Admin-only and includes score changes, event administration,
flight changes, publication changes, and explicit legacy-identity migration.

Audited mutations append history atomically to the affected MongoDB document.
Audited domain deletion is logical, preserving the record and its history until
the approved audit-retention period expires. Database operators remain privileged;
application controls do not provide tamper-proof storage against database administrators.

## Audit Retention

The operational policy is 24 calendar months after the event's recorded UTC end.
Admins record the actual end on Audit History; doing so closes the event to further
judging. Ordinary event edits cannot clear the end date or reactivate a completed
event. Existing events without an explicit end never expire automatically; do not
infer an end from the event year, registration deadline, or publication date.

A startup sweep and a daily background job remove expired embedded SecurityAudit
entries only. Admins can also request cleanup with explicit confirmation. Events,
wines, scores, payments, receipts, and their business data are not deleted by this
job. Each audit entry also has a minimum 24-month age, so newer corrections and
maintenance records remain available. Daily scheduling and calendar boundaries can
defer removal beyond the earliest eligibility time.

An unresolved dispute extends retention only when an Admin records a private case
reference as a dispute hold. Use a case identifier, not a person's name, evidence,
credentials, or the contents of the dispute. Release the hold explicitly when the
dispute is resolved. Setting, changing, and releasing holds is audited. Holds
remain visible to Admins only and do not extend accounting-record retention.

Cleanup and hold/end changes share a per-event database lock. Cleanup rechecks the
hold and end date after acquiring it. There is deliberately no automatic lock expiry:
a crash or uncertain database response must not allow cleanup to race an accepted
hold. A busy or abandoned lock is an operational error, not permission to bypass
the hold. Failed or interrupted sweeps may have partially removed eligible history;
retry is safe, and failures must be investigated using server logs.

### Backup and Restore Runbook

Backup infrastructure is not configured or managed by this repository. Before
rollout, the database operator must configure and verify expiry or approved
crypto-erasure of backup copies containing expired audit history. Application
cleanup alone does not remove data from backups, snapshots, exports, logs, or
independent archives. Keep a documented inventory, expiry schedule, and deletion
evidence outside the application database; do not silently retain indefinite copies.

Restore backups into an isolated environment with no user access. Reconcile current
event end dates, dispute holds, and completed deletions from the external operational
register before enabling the cleanup job or user access. Apply the retention sweep
and verify the result before reopening the restored system. If a maintenance lock
was restored or abandoned, first stop retention workers on every instance and
confirm that no Admin retention action or cleanup operation is still running. Only
then may a privileged database operator remove that event's AuditRetentionLock;
restart the application and recheck holds before cleanup. Never use a blanket lock
reset while application instances are running.

Records whose event cannot be resolved require manual review, not guessed expiry.
Backups or accounting documents subject to a distinct legal retention obligation
must be handled under a separately documented policy. The 24-month recommendation
is an operational choice, not a determination of statutory retention requirements.

See docs/plan/20261023-security-improvements/plan.yaml for rollout gates and
remaining environment-dependent verification.
