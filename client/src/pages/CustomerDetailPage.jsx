import { useState, useEffect, useCallback } from 'react';
import { useParams, Link } from 'react-router-dom';
import api from '../hooks/api';
import OrderList from '../components/OrderList';
import CreateOrderForm from '../components/CreateOrderForm';

export default function CustomerDetailPage() {
  const { id } = useParams();
  const [orders, setOrders] = useState([]);
  const [customer, setCustomer] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchData = useCallback(async () => {
    try {
      setLoading(true);
      const [ordersRes, customerRes] = await Promise.all([
        api.get(`/orders/customer/${id}`),
        api.get(`/customers/${id}`),
      ]);
      setOrders(ordersRes.data);
      setCustomer(customerRes.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  if (loading) return <div className="loading">Loading orders...</div>;
  if (error) return <div className="error-msg">{error}</div>;

  return (
    <div>
      <Link to="/" className="back-link">&larr; Back to Customers</Link>

      <div className="card">
        <div className="section-title">
          {customer?.name || `Customer #${id}`}
        </div>
        {customer && <p className="text-muted" style={{ marginBottom: 20 }}>{customer.email}</p>}
        <OrderList orders={orders} />
      </div>

      <div className="card">
        <CreateOrderForm customerId={Number(id)} onOrderCreated={fetchData} />
      </div>
    </div>
  );
}
