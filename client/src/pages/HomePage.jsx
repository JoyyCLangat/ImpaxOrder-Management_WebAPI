import { useCallback, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useCustomers } from '../hooks/useCustomers';
import { useAuth } from '../context/AuthContext';
import CustomerList from '../components/CustomerList';
import api from '../hooks/api';

export default function HomePage() {
  const { customers, loading, error, refetch } = useCustomers();
  const { username, role, logout } = useAuth();
  const navigate = useNavigate();

  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [formError, setFormError] = useState('');
  const [creating, setCreating] = useState(false);

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

  const handleCreateCustomer = async (e) => {
    e.preventDefault();
    if (!name.trim() || !email.trim()) {
      setFormError('Name and email are required');
      return;
    }
    setFormError('');
    setCreating(true);
    try {
      await api.post('/customers', { name, email });
      setName('');
      setEmail('');
      setShowForm(false);
      refetch();
    } catch (err) {
      setFormError(err.response?.data?.message || 'Failed to create customer');
    } finally {
      setCreating(false);
    }
  };

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
          <div className="stat-value">KSh {grandTotal.toFixed(2)}</div>
          <div className="stat-label">Total Revenue</div>
        </div>
      </div>

      {loading && <div className="loading">Loading customers...</div>}
      {error && <div className="error-msg">{error}</div>}
      {!loading && !error && (
        <div className="card">
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
            <div>
              <div className="section-title" style={{ marginBottom: 4 }}>Customer Overview</div>
              <p className="text-muted">Click a row to view orders and create new ones.</p>
            </div>
            {role === 'Manager' && (
              <button className="btn-primary btn-sm" onClick={() => setShowForm(!showForm)}>
                {showForm ? 'Cancel' : '+ Add Customer'}
              </button>
            )}
          </div>

          {showForm && (
            <form onSubmit={handleCreateCustomer} style={{ marginBottom: 20 }}>
              {formError && <div className="error-msg">{formError}</div>}
              <div className="form-row">
                <input
                  placeholder="Customer name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  style={{ flex: 1 }}
                />
                <input
                  type="email"
                  placeholder="Email address"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  style={{ flex: 1 }}
                />
                <button type="submit" disabled={creating} className="btn-primary btn-sm">
                  {creating ? 'Saving...' : 'Save'}
                </button>
              </div>
            </form>
          )}

          <CustomerList customers={customers} onSelectCustomer={handleSelectCustomer} />
        </div>
      )}
    </div>
  );
}
