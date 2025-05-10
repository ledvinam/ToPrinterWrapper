# TODO / NEXT STEPS

## 1. Testing & Robustness
- [ ] Add/Expand Unit Tests:
  - Ensure all public APIs, especially PrintDocumentAsync (file/stream overloads), are covered with tests for success, error, and cancellation scenarios.
  - Add tests for RAM disk fallback logic and file deletion queue behavior.
  - Add tests for error handling in process execution (e.g., 2Printer not found, printer offline, etc.).
- [ ] Test on All Supported .NET Versions:
  - Validate behavior and compatibility on .NET 6, 7, 8, and 9.

## 2. Code Quality & Maintainability
- [ ] Refactor Large Methods:
  - Consider breaking up large methods (e.g., PrintDocumentAsync) for readability and single-responsibility.
- [ ] Consistent Logging:
  - Unify logging approach (currently both Log and Silent are used; clarify their intent and document).
  - Consider using a logging abstraction (e.g., ILogger) for better testability and flexibility.
- [ ] Exception Handling:
  - Review all catch blocks. Consider logging or surfacing unexpected exceptions, not just ignoring them.
  - Optionally, add a global error handler or event for unhandled errors.

## 3. Performance & Resource Management
- [ ] Optimize Temp File Handling:
  - Use Path.GetTempFileName() or similar for temp files to avoid collisions.
  - Ensure temp files are always cleaned up, even on process crash (document limitations).
- [ ] Stream Handling:
  - For large files, consider using a configurable buffer size for CopyToAsync.

## 4. Documentation & Usability
- [ ] API Usage Examples:
  - Add code samples to README.md for common usage patterns.
- [ ] Document RAM Disk Requirements:
  - Clarify in README.md and code comments how/when RAM disk is used, and fallback behavior.
- [ ] Document All Exceptions:
  - Ensure XML docs for all public methods list possible exceptions.

## 5. Build, Packaging, and CI/CD
- [ ] Re-enable and Expand CI/CD:
  - Re-enable .github/workflows/dotnet.yml and add test matrix for all supported .NET versions.
  - Add code coverage reporting.
- [ ] NuGet Package Metadata:
  - Review and enhance .csproj for package tags, description, and project URL.

## 6. Features & Enhancements
- [ ] Printer Status Caching:
  - Optionally cache printer status for a short period to avoid repeated checks.
- [ ] Custom Print Path Option:
  - Allow users to specify a custom print path at runtime (e.g., via environment variable or config).
- [ ] Graceful Shutdown Improvements:
  - Ensure all background tasks (e.g., file delete queue) are fully drained and disposed on shutdown.

## 7. Miscellaneous
- [ ] Code Style Consistency:
  - Run code formatters and analyzers to ensure consistent style and naming.
- [ ] Review All TODO/FIXME Comments:
  - Address or document any remaining inline TODOs in the codebase.
