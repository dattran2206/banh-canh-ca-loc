import React, { useState, useMemo } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Plus, Minus, ShoppingBag } from 'lucide-react';
import { cn } from '@/lib/utils';

export default function NewOrderDialog({ table, existingOrderId = null, onClose, onCreated }) {
    const { currentUser, currentShift } = useAppAuth();
    const { createOrder, appendOrderItems, categories: dataCategories, menuItems: dataMenuItems } = useData();
    const [cart, setCart] = useState([]);
    const [activeCategory, setActiveCategory] = useState('all');
    const [activeTab, setActiveTab] = useState('menu'); // 'menu' | 'cart'

    const { categories, menuItems } = useMemo(() => {
        const cats = [{ id: 'all', name: 'Tất cả' }, ...dataCategories];
        const items = dataMenuItems.filter(m => m.isAvailable);
        return { categories: cats, menuItems: items };
    }, [dataCategories, dataMenuItems]);

    const filtered = activeCategory === 'all' ? menuItems : menuItems.filter(m => m.categoryId === activeCategory || m.categoryId.toString() === activeCategory.toString());

    const addToCart = (item) => {
        setCart(prev => {
            const ex = prev.find(c => c.menuItemId === item.id);
            if (ex) return prev.map(c => c.menuItemId === item.id ? { ...c, quantity: c.quantity + 1 } : c);
            return [...prev, { menuItemId: item.id, menuItem: item, quantity: 1, note: '' }];
        });
    };

    const removeFromCart = (menuItemId) => {
        setCart(prev => {
            const ex = prev.find(c => c.menuItemId === menuItemId);
            if (ex?.quantity === 1) return prev.filter(c => c.menuItemId !== menuItemId);
            return prev.map(c => c.menuItemId === menuItemId ? { ...c, quantity: c.quantity - 1 } : c);
        });
    };

    const updateNote = (menuItemId, note) => {
        setCart(prev => prev.map(c => c.menuItemId === menuItemId ? { ...c, note } : c));
    };

    const handleConfirm = () => {
        if (cart.length === 0) return;

        const orderItemsDto = cart.map(item => ({
            menuItemId: item.menuItemId,
            quantity: item.quantity,
            note: item.note || ''
        }));

        if (!existingOrderId) {
            createOrder({
                tableId: table.id,
                shiftId: currentShift,
                items: orderItemsDto
            }).then(() => {
                onCreated();
            }).catch(err => {
                console.error("Error creating order", err);
            });
        } else {
            appendOrderItems(existingOrderId, orderItemsDto).then(() => {
                onCreated();
            }).catch(err => {
                console.error("Error adding items to order", err);
            });
        }
    };

    const totalAmount = cart.reduce((s, c) => s + c.menuItem.price * c.quantity, 0);

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-2xl max-h-[90vh] flex flex-col">
                <DialogHeader>
                    <DialogTitle className="text-amber-900">
                        {existingOrderId ? `Thêm món — Bàn ${table.number}` : `Order mới — Bàn ${table.number}`}
                    </DialogTitle>
                </DialogHeader>

                {/* Mobile Tab Switcher */}
                <div className="flex md:hidden bg-amber-100 p-1 rounded-xl mb-2 flex-shrink-0">
                    <button 
                        onClick={() => setActiveTab('menu')} 
                        className={cn(
                            "flex-1 py-1.5 text-xs font-semibold rounded-lg transition-colors", 
                            activeTab === 'menu' ? 'bg-amber-500 text-white' : 'text-amber-800'
                        )}
                    >
                        Thực đơn
                    </button>
                    <button 
                        onClick={() => setActiveTab('cart')} 
                        className={cn(
                            "flex-1 py-1.5 text-xs font-semibold rounded-lg transition-colors", 
                            activeTab === 'cart' ? 'bg-amber-500 text-white' : 'text-amber-800'
                        )}
                    >
                        Giỏ hàng ({cart.length})
                    </button>
                </div>

                <div className="flex flex-col md:flex-row gap-4 flex-1 overflow-hidden">
                    {/* Menu */}
                    <div className={cn("flex-1 flex flex-col overflow-hidden", activeTab === 'menu' ? 'flex' : 'hidden md:flex')}>
                        <div className="flex gap-2 overflow-x-auto pb-2 flex-shrink-0">
                            {categories.map(cat => (
                                <button
                                    key={cat.id}
                                    onClick={() => setActiveCategory(cat.id)}
                                    className={cn(
                                        'px-3 py-1.5 rounded-full text-xs font-medium whitespace-nowrap transition-colors',
                                        activeCategory.toString() === cat.id.toString() ? 'bg-amber-500 text-white' : 'bg-amber-100 text-amber-700 hover:bg-amber-200'
                                    )}
                                >
                                    {cat.name}
                                </button>
                            ))}
                        </div>
                        <div className="overflow-y-auto flex-1 grid grid-cols-2 gap-2 mt-2">
                            {filtered.map(item => {
                                const cartItem = cart.find(c => c.menuItemId === item.id);
                                return (
                                    <button
                                        key={item.id}
                                        onClick={() => addToCart(item)}
                                        className="relative border border-amber-200 rounded-xl p-3 text-left hover:bg-amber-50 transition-colors active:scale-95"
                                    >
                                        <p className="font-medium text-sm text-amber-900 leading-tight">{item.name}</p>
                                        <p className="text-amber-600 text-sm font-bold mt-1">{item.price.toLocaleString('vi-VN')}đ</p>
                                        {cartItem && (
                                            <Badge className="absolute top-2 right-2 bg-amber-500 text-white text-xs">
                                                {cartItem.quantity}
                                            </Badge>
                                        )}
                                    </button>
                                );
                            })}
                        </div>
                    </div>

                    {/* Cart */}
                    <div className={cn("w-full md:w-56 flex flex-col border-t md:border-t-0 md:border-l border-amber-100 pt-4 md:pt-0 pl-0 md:pl-4 mt-4 md:mt-0", activeTab === 'cart' ? 'flex' : 'hidden md:flex')}>
                        <p className="font-semibold text-amber-800 text-sm mb-2 flex items-center gap-1">
                            <ShoppingBag className="w-4 h-4" /> Đã chọn ({cart.length})
                        </p>
                        <div className="flex-1 overflow-y-auto space-y-2">
                            {cart.map(item => (
                                <div key={item.menuItemId} className="border border-amber-100 rounded-lg p-2">
                                    <div className="flex items-center justify-between">
                                        <span className="text-xs font-medium text-amber-900 flex-1 mr-1">{item.menuItem.name}</span>
                                        <div className="flex items-center gap-1">
                                            <button onClick={() => removeFromCart(item.menuItemId)} className="w-5 h-5 rounded-full bg-amber-100 flex items-center justify-center">
                                                <Minus className="w-3 h-3 text-amber-700" />
                                            </button>
                                            <span className="text-xs font-bold w-4 text-center">{item.quantity}</span>
                                            <button onClick={() => addToCart(item.menuItem)} className="w-5 h-5 rounded-full bg-amber-100 flex items-center justify-center">
                                                <Plus className="w-3 h-3 text-amber-700" />
                                            </button>
                                        </div>
                                    </div>
                                    <Input
                                        value={item.note}
                                        onChange={e => updateNote(item.menuItemId, e.target.value)}
                                        placeholder="Ghi chú..."
                                        className="mt-1 h-6 text-xs border-amber-200 px-2"
                                    />
                                </div>
                            ))}
                            {cart.length === 0 && (
                                <p className="text-xs text-amber-400 text-center py-4">Chưa chọn món nào</p>
                            )}
                        </div>
                        <div className="border-t border-amber-100 pt-2 mt-2">
                            <div className="flex justify-between text-sm font-bold text-amber-900 mb-2">
                                <span>Tổng</span>
                                <span>{totalAmount.toLocaleString('vi-VN')}đ</span>
                            </div>
                            <Button
                                onClick={handleConfirm}
                                disabled={cart.length === 0}
                                className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl h-10 text-sm"
                            >
                                Xác nhận order
                            </Button>
                        </div>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}