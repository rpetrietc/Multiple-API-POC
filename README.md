# Multiple API POC

## Overview

This proof-of-concept demonstrates technical patterns for applications that need to retrieve information from multiple backend systems or Application Programming Interfaces (APIs).

The application focuses on two main concerns:

1. **Performance**
   - How to reduce response time when multiple independent APIs need to be called.
   - How caching can reduce repeated backend requests.

2. **Resilience**
   - How an application can continue operating when one or more backend services are slow, temporarily unavailable, or failing.

The overall goal is to demonstrate how a web application can retrieve data efficiently without allowing one slow or unavailable dependency to negatively affect the entire user experience.

---

## Technology

- ASP.NET Core MVC
- .NET 8
- HttpClient / IHttpClientFactory
- async / await
- Task.WhenAll
- HybridCache
- Microsoft.Extensions.Http.Resilience

---

# API Performance Demo

The main demo compares three approaches to retrieving information from multiple independent APIs.

## Sequential API Calls

Each API call completes before the next call begins.

Example:

```text
API 1
  ↓
wait
  ↓
API 2
  ↓
wait
  ↓
API 3
  ↓
wait
  ↓
API 4
```

### Pros

- Simple and easy to understand.
- Useful when one API call depends on the result of another.
- Easier to follow and troubleshoot.

### Cons

- Slower when API calls are independent.
- Total response time is approximately the sum of the individual API call times.
- One slow API can delay everything that follows it.

---

## Parallel API Calls

Independent API calls are started at approximately the same time and awaited together using `Task.WhenAll`.

Example:

```text
API 1 ─┐
API 2 ─┤
API 3 ─┼── Task.WhenAll
API 4 ─┘
```

The total response time is generally closer to the slowest individual API call rather than the sum of every API call.

### Pros

- Independent API calls run at the same time.
- Can significantly reduce overall response time.
- Useful when information needs to be gathered from several unrelated systems.

### Cons

- Should only be used when the API calls are independent.
- Creates multiple outbound requests at the same time.
- Error handling can be more complex if one API fails.

---

## Cached API Calls

The cached scenario combines parallel API calls with .NET HybridCache.

The application first checks whether suitable data is already available in the cache.

```text
Request
   ↓
Check Cache
   ↓
Cache Hit?
   │
   ├── Yes → Return cached data
   │
   └── No  → Call API
               ↓
             Cache result
               ↓
             Return data
```

The first request may still need to call the backend APIs.

Subsequent requests can return cached information until the configured cache duration expires.

### Pros

- Previously retrieved information can be returned very quickly.
- Reduces unnecessary API calls.
- Reduces load on backend systems.
- Can significantly improve application response time.

### Cons

- The first request still requires the APIs when the cache is empty.
- Cached information may become slightly out of date.
- Cache duration must be selected based on how current the information needs to be.

Different types of information may require different cache durations.

For example:

```text
User profile             → Longer cache
Status information       → Shorter cache
Notifications            → Very short cache
Static reference data    → Longer cache
```

---

# API Resilience Demo

The resilience portion of the application demonstrates what can happen when one of an application's backend dependencies is slow or unavailable.

Simulated failures are used so the scenarios behave consistently during demonstrations without relying on a real public API being unavailable.

---

## Permanent Failure

A simulated backend service continually returns:

```text
503 Service Unavailable
```

This demonstrates that an unavailable dependency can be detected and handled rather than assuming every backend service will always respond successfully.

---

## Temporary Failure and Retry

A simulated service temporarily fails and then recovers.

Example:

```text
Attempt 1 → 503 Service Unavailable
Attempt 2 → 503 Service Unavailable
Attempt 3 → 200 OK
```

This demonstrates how transient failures can be retried automatically and recover without requiring the user to manually try again.

---

## Timeout

A simulated backend service deliberately takes too long to respond.

Instead of allowing that service to delay the application indefinitely, a timeout can stop waiting after a configured period.

Example:

```text
Backend service
     ↓
Taking too long...
     ↓
Timeout reached
     ↓
Application stops waiting
```

This helps prevent one slow dependency from causing excessive response times.

---

## Circuit Breaker

A circuit breaker can temporarily stop requests to a backend service that is repeatedly failing.

Example:

```text
Request 1 → Failure
Request 2 → Failure
              ↓
        Circuit opens
              ↓
Next request → Fail quickly
```

This avoids repeatedly calling a service that is already known to be unhealthy.

After an appropriate period, the application can allow the service another opportunity to recover.

---

## Partial Success / Graceful Degradation

Multiple independent services can be called in parallel even when one dependency is unavailable.

Example:

```text
API 1                   → SUCCESS
API 2                   → SUCCESS
API 3                   → SUCCESS
Unavailable API         → FAILED
```

The important result is that the healthy services can still return useful information.

One unavailable backend service should not necessarily prevent the entire application or page from functioning.

---

# Public APIs Used

The performance portion of the proof-of-concept uses free public APIs so the application can be demonstrated without requiring authentication or access to internal infrastructure.

| API | Purpose |
| --- | --- |
| Open-Meteo | Weather data |
| Nager.Date | Canadian public holidays |
| PokéAPI | Sample Pokémon data |
| Dog CEO API | Sample dog data |

These APIs are used only to demonstrate technical behaviour.

They can be replaced with other APIs depending on the needs of a project.

---

# Technical Pattern Demonstrated

The proof-of-concept demonstrates the following general architecture:

```text
User requests page
      ↓
Application determines required information
      ↓
Check cached information
      ↓
Determine what information is missing or expired
      ↓
Call independent backend APIs in parallel
      ↓
Apply timeout / retry / circuit breaker behaviour
      ↓
Aggregate successful responses
      ↓
Cache suitable results
      ↓
Render page
```

The overall technical pattern is:

> **Parallel API calls + appropriate caching + resilience + graceful degradation**

Sequential API calls should still be used when one request genuinely depends on the result of another.

Caching should only be used when slightly stale information is acceptable, and cache duration should be selected according to the type of data being stored.

---

# Key Findings

This proof-of-concept demonstrates that:

- Independent API calls do not need to be executed sequentially.
- `Task.WhenAll` can reduce overall waiting time when calls are independent.
- Caching can reduce repeated API calls and improve subsequent response times.
- Timeouts can prevent slow services from delaying an application indefinitely.
- Retries can recover from short-lived service failures.
- Circuit breakers can stop repeated calls to persistently unhealthy services.
- One unavailable service does not necessarily need to prevent the rest of the page or application from functioning.

---

# Running the Application

From the web project directory:

```powershell
cd .\Multiple-API-POC.Web
dotnet watch run
```

`dotnet watch run` is useful during development because changes to Razor Views can be reflected without manually stopping and restarting the application.

The default HTTPS development address is:

```text
https://localhost:7045
```

---

# Conclusion

Applications that depend on multiple backend systems do not need to process every request sequentially or fail completely when an individual dependency is unavailable.

Independent API calls can be executed concurrently, suitable results can be cached, and resilience policies can reduce the impact of slow or unhealthy backend services.

A useful general design approach is:

```text
Parallel calls
     +
Caching
     +
Timeouts
     +
Retries
     +
Circuit breakers
     +
Graceful degradation
```

The exact combination of these techniques should depend on the requirements of the application, the importance of each backend dependency, and how current the returned information needs to be.
