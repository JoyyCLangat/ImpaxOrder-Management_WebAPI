import { useState } from 'react';
import api from '../hooks/api';

let nextItemId = 1;

export default function CreateOrderForm({ customerId, onOrderCreated }) {
  const [lineItems, setLineItems] = useState([
    { id: nextItemId++, productName: '', quantity: '', unitPrice: '' },
  ]);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const updateItem = (index, field, value) => {
    setLineItems((prev) =>
      prev.map((item, i) =>
        i === index ? { ...item, [field]: value } : item
      )
    );
  };

  const addItem = () => {
    setLineItems((prev) => [...prev, { id: nextItemId++, productName: '', quantity: '', unitPrice: '' }]);
  };

  const removeItem = (index) => {
    setLineItems((prev) => prev.filter((_, i) => i !== index));
  };

  const validate = () => {
    for (const item of lineItems) {
      if (!item.productName.trim()) return 'Product name is required';
      if (item.quantity < 1) return 'Quantity must be at least 1';
      if (item.unitPrice <= 0) return 'Price must be greater than 0';
    }
    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const validationError = validate();
    if (validationError) {
      setError(validationError);
      return;
    }

    setError('');
    setSubmitting(true);
    try {
      const idempotencyKey = `ORD-${Date.now()}-${Math.random().toString(36).slice(2, 7)}`;
      await api.post('/orders', {
        idempotencyKey,
        customerId,
        lineItems: lineItems.map((li) => ({
          productName: li.productName,
          quantity: Number(li.quantity),
          unitPrice: Number(li.unitPrice),
        })),
      });
      setLineItems([{ id: nextItemId++, productName: '', quantity: '', unitPrice: '' }]);
      onOrderCreated();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create order');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <div className="section-title">Create New Order</div>
      {error && <div className="error-msg">{error}</div>}

      {lineItems.map((item, index) => (
        <div key={item.id} className="form-row">
          <input
            placeholder="Product name"
            value={item.productName}
            onChange={(e) => updateItem(index, 'productName', e.target.value)}
            style={{ flex: 2 }}
          />
          <input
            type="number"
            placeholder="Quantity"
            min="1"
            value={item.quantity}
            onChange={(e) => updateItem(index, 'quantity', e.target.value)}
            style={{ flex: 0, width: 80 }}
          />
          <input
            type="number"
            placeholder="Price (KSh)"
            min="0.01"
            step="0.01"
            value={item.unitPrice}
            onChange={(e) => updateItem(index, 'unitPrice', e.target.value)}
            style={{ flex: 0, width: 100 }}
          />
          {lineItems.length > 1 && (
            <button type="button" onClick={() => removeItem(index)} className="btn-danger btn-sm">
              Remove
            </button>
          )}
        </div>
      ))}

      <div className="form-actions">
        <button type="button" onClick={addItem} className="btn-secondary">
          + Add Item
        </button>
        <button type="submit" disabled={submitting} className="btn-primary">
          {submitting ? 'Creating...' : 'Create Order'}
        </button>
      </div>
    </form>
  );
}
