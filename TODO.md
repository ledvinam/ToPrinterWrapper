# TODO: Project Improvements

## 1. Code Quality & Maintainability
- Refactor `ToPrinter` class to separate process management, file deletion, and printer status into smaller classes.
- Replace `Console.WriteLine` with Microsoft.Extensions.Logging for better logging.
- Make more settings (e.g., 2Printer path, log level) configurable via DI or appsettings.
- Improve error handling and reporting, consider custom exceptions.
- Ensure all async resources are disposed properly (e.g., implement IAsyncDisposable where needed).

## 2. Testing
- Add negative tests (invalid arguments, missing files, permission errors).
- Add tests for error codes and edge cases.
- Mock 2Printer process for unit tests to avoid dependency on the actual executable.
- Enable test parallelization for unrelated tests if possible.

## 3. Performance
- Consider using BlockingCollection or Channel in FileDeleteQueue for better producer/consumer pattern.
- Ensure semaphore is not over-released in error cases.
- Use `await using` for streams to ensure proper disposal.

## 4. Documentation
- Add example error handling and troubleshooting section to README.
- Add contribution guidelines if open source.
- Ensure all public APIs are fully documented, including exceptions.

## 5. Usability & API Design
- Consider a fluent builder for PrintOptions.
- Make cancellation and timeout parameters consistent across all async methods.
- Consider returning a result object (output, error, exit code) instead of just exit code.

## 6. Robustness
- Allow specifying full path to 2Printer.exe for non-standard installs.
- Validate and fallback if RAM disk is not available.
- Ensure all process output is read even on cancellation/kill to avoid deadlocks.

## 7. Build & Packaging
- Ensure symbols and XML docs are included in NuGet package.
- Add CI/CD workflow for build, test, and package validation.
- Consider automated versioning (e.g., GitVersion).

## 8. Miscellaneous
- Ensure .gitignore covers all generated files.
- Add error handling and comments to .cmd scripts for maintainability.
