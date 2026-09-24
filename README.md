# Order Management System

A small order management app with an ASP.NET Core API and a React frontend.

## How to run

**Backend:** `cd src/OrderManagement.API && dotnet run`
**Frontend:** `cd client && npm install && npm run dev`
**Logins:** `admin/admin123` (Manager) · `user/user123` (User)

## Secrets

The JWT key and connection string live in `appsettings.Development.json` for local dev and in environment variables for production. Nothing sensitive is committed. For a real deployment I'd use dotnet user-secrets or a vault.

## Backend

I split the code into four projects (Domain, Application, Infrastructure, API) so that each layer only knows about the one below it. Services are wired up through dependency injection and every database call is async. The API is protected by JWT tokens and only managers can access the report endpoint. If someone tries to create a customer with an existing email or an order with a duplicate idempotency key, the database catches it and the API returns a clear error instead of crashing.

**Report:** `GET /api/customers/report` gives you every customer with how many orders they have and how much they spent — even if they have zero orders.

**SQL queries:**

1. **Customer spending (LEFT JOIN)** — shows all customers, even those with no orders:
   ```sql
   SELECT c.Id, c.Name, c.Email, COUNT(o.Id) AS OrderCount,
          COALESCE(SUM(li.Quantity * li.UnitPrice), 0) AS TotalSpend
   FROM Customers c
   LEFT JOIN Orders o ON o.CustomerId = c.Id
   LEFT JOIN LineItems li ON li.OrderId = o.Id
   GROUP BY c.Id, c.Name, c.Email
   ```

2. **Orders by status (INNER JOIN)** — only returns orders that match, with customer info attached:
   ```sql
   SELECT o.*, c.Name, c.Email
   FROM Orders o
   INNER JOIN Customers c ON c.Id = o.CustomerId
   WHERE o.Status = @status
   ```

**Index:** I'd put an index on `Orders.Status` since that's the column we filter on most.

## Frontend (React)

I wrote a custom `useCustomers()` hook that handles fetching data, loading state, and errors all in one place. For sharing the logged-in user and token across components I went with Context and useReducer — I didn't see a reason to bring in Redux or Zustand when the shared state is just one token and a user object. The customer detail page is lazy-loaded so it only downloads when you actually visit it. Forms check input before sending anything to the API. The JWT token gets attached to every request automatically, and if the server says you're not logged in (401) it sends you back to the login page. Lists use the actual record ID as the key, not the array position.

**Memoization:** I wrapped customer rows with `React.memo` so that when the parent re-renders, rows that haven't changed don't re-render too. You can check this in React DevTools Profiler — unchanged rows will say "did not render."

## Scaling

If this app had 10x the data I'd add server-side pagination first so we're not loading everything at once, then use something like react-window to only render the rows on screen, and swap out the manual fetch hooks for React Query to get caching and background refreshes.
