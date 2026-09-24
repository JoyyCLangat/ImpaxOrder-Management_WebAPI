# Order Management System

ASP.NET Core Web API + React.

## Run

**Backend**

```bash
cd src/OrderManagement.API
dotnet run
```

**Frontend**

```bash
cd client
npm install
npm run dev
```

Login: `admin/admin123` (Manager) | `user/user123` (User)

## Backend

* Clean Architecture
* Dependency Injection + async/await
* JWT authentication + role protection
* Duplicate customers/orders prevented
* Customer report uses LEFT JOIN
* Order status report uses INNER JOIN
* Index on `Orders.Status`

## Frontend

* React hooks + custom `useCustomers()` hook
* Context + useReducer for authentication because it is simple and built into React; no need for Redux/Zustand for this small app.
* Lazy-loaded customer details
* Form validation
* JWT requests
* React.memo on customer rows to prevent unnecessary re-renders when the parent updates. I would verify this using React DevTools Profiler.
* Reusable components

## Scaling

With 10x the data, I would add pagination, virtualization, and React Query.
