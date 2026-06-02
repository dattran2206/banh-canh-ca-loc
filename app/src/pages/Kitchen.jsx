import React, { useState, useMemo, useEffect } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ChefHat, Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useToast } from '@/components/ui/use-toast';

export default function Kitchen() {
    const { currentUser } = useAppAuth();
    const { orders, updateOrderStatus } = useData();
    const { toast } = useToast();
    const [time, setTime] = useState(Date.now());

    useEffect(() => {
        const interval = setInterval(() => setTime(Date.now()), 10000);
        return () => clearInterval(interval);
    }, []);

    const kitchenOrders = useMemo(() => {
        return [...orders]
            .filter(o => ['confirmed', 'preparing'].includes(o.status))
            .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
    }, [orders]);

    const handleStatusUpdate = async (orderId, newStatus) => {
        try {
            const data = await updateOrderStatus(orderId, newStatus);
            if (newStatus === 'ready') {
                if (data.warnings && data.warnings.length > 0) {
                    toast({
                        variant: "destructive",
                        title: "Cảnh báo tồn kho thấp",
                        description: data.warnings.join(', '),
                    });
                }
            }
        } catch (err) {
            console.error("Failed to update order status", err);
        }
    };

    const elapsed = (createdAt) => {
        const mins = Math.floor((time - new Date(createdAt).getTime()) / 60000);
        return `${mins} phút`;
    };

    return (
        <div className="min-h-screen bg-gray-900 p-4">
            <div className="flex items-center gap-3 mb-6">
                <ChefHat className="w-8 h-8 text-amber-400" />
                <h1 className="text-2xl font-bold text-white">Bếp — Danh sách order</h1>
                <Badge className="bg-amber-500 text-white">{kitchenOrders.length} order</Badge>
            </div>

            {kitchenOrders.length === 0 ? (
                <div className="text-center py-20 text-gray-500">
                    <ChefHat className="w-16 h-16 mx-auto mb-4 opacity-30" />
                    <p className="text-xl">Không có order nào cần chế biến</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
                    {kitchenOrders.map(order => (
                        <div
                            key={order.id}
                            className={cn(
                                'rounded-2xl border-2 p-4 flex flex-col gap-3',
                                order.status === 'confirmed' ? 'bg-gray-800 border-blue-500' : 'bg-gray-800 border-amber-500'
                            )}
                        >
                            <div className="flex items-center justify-between">
                                <div>
                                    <p className="text-white text-xl font-bold">
                                        Bàn {order.table?.number} — {order.orderNumber}
                                    </p>
                                    <div className="flex items-center gap-2 mt-1">
                                        <Badge className={cn(
                                            'text-sm px-3 py-1',
                                            order.status === 'confirmed' ? 'bg-blue-600 text-white' : 'bg-amber-500 text-white'
                                        )}>
                                            {order.status === 'confirmed' ? 'Chờ chế biến' : 'Đang chế biến'}
                                        </Badge>
                                        <span className="text-gray-400 text-sm flex items-center gap-1">
                                            <Clock className="w-3 h-3" /> {elapsed(order.createdAt)}
                                        </span>
                                    </div>
                                </div>
                            </div>

                            <div className="space-y-2">
                                {(order.items || []).map(item => (
                                    <div key={item.id} className="bg-gray-700 rounded-xl px-3 py-2">
                                        <div className="flex justify-between items-start">
                                            <span className="text-white text-lg font-semibold">{item.menuItem?.name}</span>
                                            <span className="text-amber-400 text-xl font-bold ml-2">×{item.quantity}</span>
                                        </div>
                                        {item.note && (
                                            <p className="text-yellow-400 text-sm mt-0.5">📝 {item.note}</p>
                                        )}
                                    </div>
                                ))}
                            </div>

                            <div className="flex gap-2 mt-auto">
                                {order.status === 'confirmed' && (
                                    <Button
                                        onClick={() => handleStatusUpdate(order.id, 'preparing')}
                                        className="flex-1 bg-amber-500 hover:bg-amber-600 text-white rounded-xl h-12 text-base font-bold"
                                    >
                                        Bắt đầu chế biến
                                    </Button>
                                )}
                                {order.status === 'preparing' && (
                                    <Button
                                        onClick={() => handleStatusUpdate(order.id, 'ready')}
                                        className="flex-1 bg-green-500 hover:bg-green-600 text-white rounded-xl h-12 text-base font-bold"
                                    >
                                        ✓ Xong — Sẵn sàng
                                    </Button>
                                )}
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}