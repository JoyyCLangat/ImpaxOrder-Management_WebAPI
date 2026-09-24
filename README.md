# Order Management System

A small order management app with an ASP.NET Core API and a React frontend.

## How to run

**Backend:** `cd src/OrderManagement.API && dotnet run`
**Frontend:** `cd client && npm install && npm run dev`
**Logins:** `admin/admin123` (Manager) · `user/user123` (User)

## Secrets

JWT key and connection string come from environment variables in production and `appsettings.Development.json` locally. Nothing sensitive is committed.

## Backend

Clean Architecture (Domain, Application, Infrastructure, API), dependency injection, fully async, JWT auth (report is Manager-only), duplicate prevention on emails and idempotency keys.

**Report:** `GET /api/customers/report`: each customer's order count and total spend, including those with zero orders.

**SQL queries:**

1. **Customer spending (LEFT JOIN)**: shows all customers, even those with no orders:
   ```sql
   SELECT c.Id, c.Name, c.Email, COUNT(o.Id) AS OrderCount,
          COALESCE(SUM(li.Quantity * li.UnitPrice), 0) AS TotalSpend
   FROM Customers c
   LEFT JOIN Orders o ON o.CustomerId = c.Id
   LEFT JOIN LineItems li ON li.OrderId = o.Id
   GROUP BY c.Id, c.Name, c.Email
   ```

2. **Orders by status (INNER JOIN)**: only returns orders that match, with customer info attached:
   ```sql
   SELECT o.*, c.Name, c.Email
   FROM Orders o
   INNER JOIN Customers c ON c.Id = o.CustomerId
   WHERE o.Status = @status
   ```

**Index:** `Orders.Status`: most filtered column.

## Frontend (React)

Custom `useCustomers()` hook, Context + useReducer for auth (picked over Redux/Zustand since the shared state is just a token), lazy-loaded customer detail, validated forms, auto-attached JWT with 401 redirect, and entity IDs as list keys.

**Memoization:** `React.memo` on customer rows so unchanged rows skip re-rendering, verifiable in React DevTools Profiler.

## Scaling

With 10x the data I'd add server-side pagination, virtualise long lists with react-window, and use React Query for caching.
