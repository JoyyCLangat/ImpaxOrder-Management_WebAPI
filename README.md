# Order Management System

ASP.NET Core Web API + React frontend.

## Setup

**Backend:** `cd src/OrderManagement.API && dotnet run`
**Frontend:** `cd client && npm install && npm run dev`
**Logins:** `admin/admin123` (Manager) · `user/user123` (User)

## Secrets

The JWT key and DB connection string come from environment variables in production and `appsettings.Development.json` locally — nothing secret is committed. In a real app I'd use `dotnet user-secrets` or a vault.

## Backend

Four projects (Domain, Application, Infrastructure, API) following Clean Architecture — inner layers don't depend on outer ones. Services are registered with dependency injection and all database calls are async. JWT protects the API; the report endpoint is restricted to the Manager role. Duplicate customers (same email) and duplicate orders (same idempotency key) are blocked by database constraints and return friendly errors instead of 500s.

**Reporting endpoint:** `GET /api/customers/report` returns every customer with their order count and total spend, including customers with zero orders.

**SQL queries:**

1. **Total spend per customer (LEFT JOIN)** — customers with no orders show up with 0:
   ```sql
   SELECT c.Id, c.Name, c.Email, COUNT(o.Id) AS OrderCount,
          COALESCE(SUM(li.Quantity * li.UnitPrice), 0) AS TotalSpend
   FROM Customers c
   LEFT JOIN Orders o ON o.CustomerId = c.Id
   LEFT JOIN LineItems li ON li.OrderId = o.Id
   GROUP BY c.Id, c.Name, c.Email
   ```

2. **Orders by status with customer details (INNER JOIN)**:
   ```sql
   SELECT o.*, c.Name, c.Email
   FROM Orders o
   INNER JOIN Customers c ON c.Id = o.CustomerId
   WHERE o.Status = @status
   ```

**Index:** `Orders.Status` — most filtered column, speeds up query 2.

## Frontend (React)

I use `useState`, `useEffect`, and a custom `useCustomers()` hook that handles fetching, loading, and errors in one place. Auth state is shared through Context + `useReducer` — I chose this over Redux/Zustand because the app only shares a token and user object, so a full library would be overkill. The customer detail page is lazy-loaded with `React.lazy` and shows a spinner while loading. Forms are controlled components with validation before submitting. The JWT token is attached to every request automatically, and a 401 redirects to login. Components are split into containers (fetch data) and presentational (display it), and lists use entity IDs as keys, never array indexes.

**Memoization:** I wrapped customer rows in `React.memo` so only the row that changed re-renders, not the whole list. You can confirm this in React DevTools Profiler — unchanged rows show "did not render."

## Scaling

With 10x the data I'd add server-side pagination first, then virtualise long lists with react-window, and replace manual fetch hooks with React Query for caching.
