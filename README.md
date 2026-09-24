# Order Management System

ASP.NET Core API + React frontend. Requires **.NET 9 SDK** and **Node.js 18+**.

**Backend:** `cd src/OrderManagement.API && dotnet run` → runs at `http://localhost:5000` (SQLite, auto-seeded).
**Frontend:** `cd client && npm install && npm run dev` → runs at `http://localhost:5173`.
**Logins:** `admin/admin123` (Manager) | `user/user123` (User).

## Backend (Part A)

The code is split into 4 projects following Clean Architecture — Domain has the models, Application has the business logic, Infrastructure talks to the database, and API handles requests. Each layer only knows about the one below it. Services are registered in `Program.cs` and passed into controllers through their constructors. Every database call uses `async/await` so nothing blocks. The API uses JWT tokens for login — only Managers can create customers. If you try to create a duplicate order (same idempotency key) or customer (same email), the API gives you a `409 Conflict` error instead of crashing. In production, secrets like the JWT key come from environment variables, not from config files.

### SQL Queries

**1. Customer report (LEFT JOIN):** Gets every customer with their order count and total spend. Uses LEFT JOIN so customers with no orders still show up.
```sql
SELECT c.Id, c.Name, c.Email, COUNT(DISTINCT o.Id) AS OrderCount,
       COALESCE(SUM(li.Quantity * li.UnitPrice), 0) AS TotalSpend
FROM Customers c LEFT JOIN Orders o ON c.Id = o.CustomerId
LEFT JOIN LineItems li ON o.Id = li.OrderId GROUP BY c.Id, c.Name, c.Email;
```
**2. Orders by status (INNER JOIN):** Gets orders with a specific status along with their customer info. Uses INNER JOIN since every order always has a customer.
```sql
SELECT o.Id, o.OrderDate, o.Status, c.Name, c.Email FROM Orders o
INNER JOIN Customers c ON o.CustomerId = c.Id WHERE o.Status = @status;
```
**Index:** I'd add an index on `Orders.Status` since we filter by status a lot and it makes that query faster.

## Frontend — React (Part B)

I made a custom hook called `useCustomers()` that fetches customer data and tracks loading/error states. The customer detail page loads only when you click on a customer using `React.lazy()` with a spinner. Auth state (token, role) is shared using Context + useReducer — I picked this over Redux or Zustand because it's built into React and this app only needs to share login info. I used `React.memo` on table rows so they don't re-render for no reason (you can see this working in React DevTools Profiler), and `useMemo`/`useCallback` to avoid unnecessary recalculations. The order form checks inputs before submitting. An Axios interceptor attaches the token to every request and sends you to the login page if your session expires. Pages like `HomePage` fetch the data, and smaller components like `CustomerList` just display what they're given. All lists use proper `id` keys and state is never mutated directly.

## If This App Had 10x the Data

I'd add pagination so the API doesn't send everything at once, use virtualized lists so the browser only draws visible rows, and add React Query to avoid fetching the same data over and over.
