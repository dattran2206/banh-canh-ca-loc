import React, { useState, useMemo, useRef } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Printer } from 'lucide-react';

export default function PaymentDialog({ orderId, table, onClose, onPaid }) {
    const { currentUser } = useAppAuth();
    const { orders, shopInfo, createPayment } = useData();
    const [discountType, setDiscountType] = useState('%');
    const [discountValue, setDiscountValue] = useState(0);
    const printRef = useRef(null);

    const { order, items } = useMemo(() => {
        const order = orders.find(o => o.id === orderId);
        const items = order?.items || [];
        return { order, items };
    }, [orders, orderId]);

    const subtotal = items.reduce((s, i) => s + (i.menuItem?.price || 0) * i.quantity, 0);
    const discountAmount = discountType === '%'
        ? Math.round(subtotal * (discountValue || 0) / 100)
        : Math.round(discountValue || 0);
    const totalAmount = Math.max(0, subtotal - discountAmount);

    const handlePrint = () => {
        const printContent = printRef.current?.innerHTML;
        const win = window.open('', '_blank', 'width=400,height=600');
        win.document.write(`
      <html><head><title>Hóa đơn</title>
      <style>
        body { font-family: monospace; font-size: 13px; padding: 16px; }
        .center { text-align: center; }
        .bold { font-weight: bold; }
        hr { border-top: 1px dashed #000; margin: 8px 0; }
        table { width: 100%; border-collapse: collapse; }
        td { padding: 2px 4px; }
        .right { text-align: right; }
        .total { font-size: 15px; font-weight: bold; }
      </style></head>
      <body>${printContent}</body></html>
    `);
        win.document.close();
        win.print();
    };

    const handlePayment = async () => {
        try {
            await createPayment({
                orderId,
                totalAmount,
                paymentMethod: 'Tiền mặt'
            });
            handlePrint();
            onPaid();
        } catch (err) {
            console.error("Payment failed", err);
        }
    };

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="text-amber-900">
                        Hóa đơn — Bàn {table?.number} {order?.orderNumber}
                    </DialogTitle>
                </DialogHeader>

                {/* Print area */}
                <div ref={printRef} className="font-mono text-sm">
                    <div className="text-center">
                        <p className="font-bold text-base">{shopInfo?.name || 'Quán Bánh Canh Cá Lóc'}</p>
                        {shopInfo?.address && <p className="text-xs">{shopInfo.address}</p>}
                        {shopInfo?.phone && <p className="text-xs">ĐT: {shopInfo.phone}</p>}
                    </div>
                    <hr className="border-dashed my-2" />
                    <p>Bàn: {table?.number} | Order: {order?.orderNumber}</p>
                    <p>Ngày: {new Date().toLocaleString('vi-VN')}</p>
                    {currentUser && <p>Thu ngân: {currentUser.fullName}</p>}
                    <hr className="border-dashed my-2" />
                    <table className="w-full text-xs">
                        <tbody>
                            {items.map(item => (
                                <tr key={item.id}>
                                    <td className="py-0.5">
                                        {item.menuItem?.name}
                                        {item.note && <span className="text-gray-500"> ({item.note})</span>}
                                        <br />× {item.quantity}
                                    </td>
                                    <td className="text-right py-0.5">{((item.menuItem?.price || 0) * item.quantity).toLocaleString('vi-VN')}đ</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                    <hr className="border-dashed my-2" />
                    <div className="flex justify-between text-sm">
                        <span>Tạm tính:</span><span>{subtotal.toLocaleString('vi-VN')}đ</span>
                    </div>
                    {discountAmount > 0 && (
                        <div className="flex justify-between text-sm text-red-600">
                            <span>Giảm giá:</span><span>-{discountAmount.toLocaleString('vi-VN')}đ</span>
                        </div>
                    )}
                    <div className="flex justify-between font-bold text-base">
                        <span>Thực thu:</span><span>{totalAmount.toLocaleString('vi-VN')}đ</span>
                    </div>
                    <hr className="border-dashed my-2" />
                    <p className="text-center text-xs">Cảm ơn quý khách! Hẹn gặp lại.</p>
                </div>

                {/* Discount controls */}
                <div className="border-t border-amber-100 pt-3 space-y-3">
                    <div className="flex items-center gap-2">
                        <Label className="text-amber-800 text-sm w-20 flex-shrink-0">Giảm giá:</Label>
                        <div className="flex gap-2 flex-1">
                            <button
                                onClick={() => setDiscountType('%')}
                                className={`px-3 py-1.5 rounded-lg text-sm font-medium border transition-colors ${discountType === '%' ? 'bg-amber-500 text-white border-amber-500' : 'border-amber-200 text-amber-700'}`}
                            >%</button>
                            <button
                                onClick={() => setDiscountType('fixed')}
                                className={`px-3 py-1.5 rounded-lg text-sm font-medium border transition-colors ${discountType === 'fixed' ? 'bg-amber-500 text-white border-amber-500' : 'border-amber-200 text-amber-700'}`}
                            >đ</button>
                            <Input
                                type="number"
                                min="0"
                                value={discountValue}
                                onChange={e => setDiscountValue(Number(e.target.value))}
                                className="flex-1 border-amber-200 h-9"
                                placeholder="0"
                             />
                        </div>
                    </div>

                    <div className="bg-amber-50 rounded-xl p-3 space-y-1 text-sm">
                        <div className="flex justify-between text-amber-700">
                            <span>Tạm tính</span><span>{subtotal.toLocaleString('vi-VN')}đ</span>
                        </div>
                        {discountAmount > 0 && (
                            <div className="flex justify-between text-red-600">
                                <span>Giảm giá</span><span>-{discountAmount.toLocaleString('vi-VN')}đ</span>
                            </div>
                        )}
                        <div className="flex justify-between font-bold text-amber-900 text-base border-t border-amber-200 pt-1">
                            <span>Thực thu</span><span>{totalAmount.toLocaleString('vi-VN')}đ</span>
                        </div>
                    </div>

                    <div className="flex gap-2">
                        <Button onClick={handlePrint} variant="outline" className="border-amber-300 text-amber-700">
                            <Printer className="w-4 h-4 mr-1" /> In thử
                        </Button>
                        <Button onClick={handlePayment} className="flex-1 bg-green-500 hover:bg-green-600 text-white rounded-xl h-10">
                            Xác nhận thanh toán
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}