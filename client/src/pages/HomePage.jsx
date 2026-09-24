import { useCallback, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCustomers } from '../hooks/useCustomers';
import { useAuth } from '../context/AuthContext';
import CustomerList from '../components/CustomerList';

export default function HomePage() {
  const { customers, loading, error } = useCustomers();
  const { username, role, logout } = useAuth();
  const navigate = useNavigate();

  const handleSelectCustomer = useCallback(
    (id) => navigate(`/customer/${id}`),
    [navigate]
  );

  // useMemo: avoid recalculating total on every render when customers haven't changed
  const grandTotal = useMemo(
    () => customers.reduce((sum, c) => sum + c.totalSpend, 0),
    [customers]
  );

  const totalOrders = useMemo(
    () => customers.reduce((sum, c) => sum + c.orderCount, 0),
    [customers]
  );

  return (
    <div>
      <div className="navbar">
        <h1>Order Management</h1>
        <div className="user-info">
          <span>{username}</span>
          <span className="role-badge">{role}</span>
          <button onClick={logout} className="btn-logout btn-sm">Logout</button>
        </div>
      </div>

      <div className="stats-row">
        <div className="stat-card">
          <div className="stat-value">{customers.length}</div>
          <div className="stat-label">Customers</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">{totalOrders}</div>
          <div className="stat-label">Total Orders</div>
        </div>
        <div className="stat-card">
          <div className="stat-value">${grandTotal.toFixed(2)}</div>
          <div className="stat-label">Total Revenue</div>
        </div>
      </div>

      {loading && <div className="loading">Loading customers...</div>}
      {error && <div className="error-msg">{error}</div>}
      {!loading && !error && (
        <div className="card">
          <div className="section-title">Customer Overview</div>
          <p className="text-muted" style={{ marginBottom: 16 }}>
            Click a row to view orders and create new ones.
          </p>
          <CustomerList customers={customers} onSelectCustomer={handleSelectCustomer} />
        </div>
      )}
    </div>
  );
}
