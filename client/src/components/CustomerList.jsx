import { memo } from 'react';

// React.memo prevents re-render when customer data hasn't changed
const CustomerRow = memo(function CustomerRow({ customer, onSelect }) {
  return (
    <tr onClick={() => onSelect(customer.id)}>
      <td style={{ fontWeight: 500 }}>{customer.name}</td>
      <td>{customer.email}</td>
      <td style={{ textAlign: 'center' }}>{customer.orderCount}</td>
      <td style={{ textAlign: 'right', fontWeight: 600 }}>KSh {customer.totalSpend.toFixed(2)}</td>
    </tr>
  );
});

export default function CustomerList({ customers, onSelectCustomer }) {
  if (customers.length === 0) {
    return <div className="empty-state">No customers found.</div>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>Name</th>
          <th>Email</th>
          <th style={{ textAlign: 'center' }}>Orders</th>
          <th style={{ textAlign: 'right' }}>Total Spend</th>
        </tr>
      </thead>
      <tbody>
        {customers.map((c) => (
          <CustomerRow key={c.id} customer={c} onSelect={onSelectCustomer} />
        ))}
      </tbody>
    </table>
  );
}
