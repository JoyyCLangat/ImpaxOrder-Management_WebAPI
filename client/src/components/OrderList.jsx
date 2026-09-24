const statusClasses = ['badge-pending', 'badge-processing', 'badge-shipped', 'badge-delivered', 'badge-cancelled'];
const statusLabels = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];

export default function OrderList({ orders }) {
  if (orders.length === 0) {
    return <div className="empty-state">No orders found for this customer.</div>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Order #</th>
          <th>Date</th>
          <th>Status</th>
          <th>Items</th>
          <th style={{ textAlign: 'right' }}>Total</th>
        </tr>
      </thead>
      <tbody>
        {orders.map((order) => (
          <tr key={order.id} style={{ cursor: 'default' }}>
            <td style={{ fontWeight: 500 }}>#{order.id}</td>
            <td>{new Date(order.orderDate).toLocaleDateString()}</td>
            <td>
              <span className={`badge ${statusClasses[order.status] || ''}`}>
                {statusLabels[order.status] || 'Unknown'}
              </span>
            </td>
            <td>
              {order.lineItems.map((li) => (
                <div key={li.id} className="line-item">
                  {li.productName} &times; {li.quantity} @ KSh {li.unitPrice.toFixed(2)}
                </div>
              ))}
            </td>
            <td style={{ textAlign: 'right', fontWeight: 600 }}>KSh {order.totalValue.toFixed(2)}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
