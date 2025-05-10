# TODO / NEXT STEPS

## 1. Print Job Max Concurrency
- **Current State:**
  - Uses a `SemaphoreSlim` to limit concurrent print jobs (`MaxConcurrentPrintingJobs`).
  - This is a robust, standard approach.
- **Possible Improvements:**
  - Dynamic Concurrency: Allow the max concurrency to be adjusted at runtime (e.g., via config reload or admin API).
  - Queue Length Monitoring: Expose metrics or logs about the current queue length and active jobs, so you can monitor if jobs are backing up.
  - Job Prioritization: If some jobs are more urgent, consider a priority queue.
  - Job Timeout: Allow a per-job or global timeout, so jobs don’t hang forever if 2Printer stalls.

## 2. Memory Pressure Handling
- **Current State:**
  - System-wide memory pressure is checked before starting a new job.
  - If above threshold, new jobs wait (polling) until memory is available.
- **Possible Improvements:**
  - Memory Pressure + Queue Length: If memory pressure is high and the queue is long, consider logging a warning or rejecting jobs after a max wait time (to avoid infinite waits).
  - Graceful Degradation: If memory pressure persists, consider reducing concurrency automatically (e.g., halve MaxConcurrentPrintingJobs temporarily).
  - Memory Pressure Events: Expose an event or callback when memory pressure is detected/relieved, for monitoring or alerting.
  - Per-Job Memory Estimation: If possible, estimate memory usage per job and throttle more aggressively for large jobs.

## 3. Other Potential Causes of Slowdown or Crashes
### a. Disk Space Exhaustion
- **Current State:**
  - Temp files are used for print jobs, but disk space is not checked.
- **Possible Improvements:**
  - Check available disk space before starting a job. Log or alert if low.

### b. 2Printer Process Hangs or Crashes
- **Current State:**
  - Timeouts and cancellation are implemented, but if 2Printer hangs in a way that doesn’t respond to kill, jobs may pile up.
- **Possible Improvements:**
  - Ensure robust process kill logic and consider a watchdog for zombie processes.

### c. FileDeleteQueue Growth
- **Current State:**
  - Locked files are queued for later deletion.
- **Possible Improvements:**
  - Monitor the size of the delete queue. If it grows too large, log a warning or alert.

### d. Unhandled Exceptions
- **Current State:**
  - ThrowExceptions and error codes are used, but unhandled exceptions in background tasks or the host process can still crash the service.
- **Possible Improvements:**
  - Add global exception handling/logging for background tasks and the host.

### e. Resource Leaks
- **Current State:**
  - Streams and handles are disposed using `using`/`await using`, but defensive coding could be improved.
- **Possible Improvements:**
  - Add more defensive coding and logging for resource cleanup.

### f. Printer/Network Issues
- **Current State:**
  - Offline/unreachable printers may cause jobs to fail or retry indefinitely.
- **Possible Improvements:**
  - Add retry/backoff logic, and optionally remove printers from rotation if they are repeatedly offline.

### g. Configuration Reloads
- **Current State:**
  - Options are set at startup.
- **Possible Improvements:**
  - Support live config reloads (e.g., via IOptionsMonitor) for concurrency/memory settings.

### h. Logging Overhead
- **Current State:**
  - Console logging is used.
- **Possible Improvements:**
  - Use a structured logging framework (e.g., Serilog, NLog) with log levels and async sinks to avoid blocking.

## 4. Monitoring & Observability
- **Current State:**
  - No metrics or health checks are exposed.
- **Possible Improvements:**
  - Expose metrics: number of active jobs, queue length, memory usage, disk space, failed jobs, etc.
  - Implement ASP.NET Core health checks for memory, disk, and printer status.
  - Integrate with monitoring/alerting systems for critical conditions.

## 5. Graceful Shutdown
- **Current State:**
  - Semaphore is drained and resources are disposed.
- **Possible Improvements:**
  - Ensure all in-flight jobs are either completed or cancelled on shutdown, and that no new jobs are accepted after shutdown starts.

## 6. Documentation & Usability
- **Current State:**
  - Basic usage and troubleshooting in README, some XML docs.
- **Possible Improvements:**
  - Add more API usage examples to README.md.
  - Clarify RAM disk requirements and fallback behavior in docs.
  - Ensure all public methods document possible exceptions.

## 7. Build, Packaging, and CI/CD
- **Current State:**
  - CI/CD workflow exists but is disabled; basic NuGet metadata.
- **Possible Improvements:**
  - Re-enable and expand CI/CD (test matrix for all .NET versions, code coverage reporting).
  - Review/enhance .csproj for package tags, description, and project URL.

## 8. Miscellaneous
- **Current State:**
  - Code style is mostly consistent, but some TODO/FIXME comments may remain.
- **Possible Improvements:**
  - Run code formatters and analyzers to ensure consistent style and naming.
  - Address or document any remaining inline TODOs in the codebase.

---

### Summary Table

| Area                | Current State         | Improvements/Ideas                                      |
|---------------------|----------------------|---------------------------------------------------------|
| Max Concurrency     | SemaphoreSlim        | Dynamic config, metrics, prioritization, timeouts       |
| Memory Pressure     | System-wide, waits   | Max wait, events, dynamic concurrency, per-job estimate |
| Disk Space          | Not checked          | Check before job, alert if low                          |
| Process Hangs       | Timeout, kill        | Watchdog, robust kill                                   |
| Delete Queue        | In-memory queue      | Monitor size, alert if large                            |
| Exceptions          | Throw/return codes   | Global handler for background tasks                     |
| Resource Leaks      | Defensive, using     | More logging, ensure all paths dispose                  |
| Printer Issues      | Status check         | Retry/backoff, remove bad printers                      |
| Config Reload       | At startup           | Support live reload                                     |
| Logging             | Console              | Structured, async, log levels                           |
| Monitoring          | None                 | Expose metrics, health checks, alerting                 |
| Shutdown            | Semaphore drain      | Block new jobs, cancel in-flight jobs                   |
