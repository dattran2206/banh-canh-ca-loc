import React, { useState, useMemo, useEffect, useRef } from 'react';
import { useAppAuth } from '@/lib/appAuth';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { ChefHat, Clock, AlertCircle, Check, Play, Flame, Eye, EyeOff, Sun } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useToast } from '@/components/ui/use-toast';

// Web Audio API Synthesizer for premium doorbell chime
const playNotificationSound = () => {
    try {
        const ctx = new (window.AudioContext || window['webkitAudioContext'])();
        if (ctx.state === 'suspended') {
            ctx.resume();
        }

        // Tone 1: C5 (523.25 Hz)
        const osc1 = ctx.createOscillator();
        const gain1 = ctx.createGain();
        osc1.connect(gain1);
        gain1.connect(ctx.destination);
        osc1.type = 'sine';
        osc1.frequency.setValueAtTime(523.25, ctx.currentTime);
        gain1.gain.setValueAtTime(0.12, ctx.currentTime);
        gain1.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.4);
        osc1.start(ctx.currentTime);
        osc1.stop(ctx.currentTime + 0.4);

        // Tone 2: E5 (659.25 Hz) after 120ms delay
        const osc2 = ctx.createOscillator();
        const gain2 = ctx.createGain();
        osc2.connect(gain2);
        gain2.connect(ctx.destination);
        osc2.type = 'sine';
        osc2.frequency.setValueAtTime(659.25, ctx.currentTime + 0.12);
        gain2.gain.setValueAtTime(0, ctx.currentTime);
        gain2.gain.setValueAtTime(0.12, ctx.currentTime + 0.12);
        gain2.gain.exponentialRampToValueAtTime(0.001, ctx.currentTime + 0.52);
        osc2.start(ctx.currentTime + 0.12);
        osc2.stop(ctx.currentTime + 0.52);

    } catch (e) {
        console.error("Failed to play KDS chime sound", e);
    }
};

export default function Kitchen() {
    const { currentUser } = useAppAuth();
    const { orders, updateOrderStatus } = useData();
    const { toast } = useToast();
    const [time, setTime] = useState(Date.now());
    const [activeTab, setActiveTab] = useState('all'); // 'all' | 'confirmed' | 'preparing'

    // Eye care and visual customization states
    const [eyeCare, setEyeCare] = useState(() => {
        const val = localStorage.getItem('kds_eyecare');
        return val !== null ? JSON.parse(val) : true;
    });
    const [dimLevel, setDimLevel] = useState(() => {
        const val = localStorage.getItem('kds_dimlevel');
        return val !== null ? parseInt(val, 10) : 15;
    });
    const [fontSize, setFontSize] = useState(() => {
        return localStorage.getItem('kds_fontsize') || 'normal';
    });

    useEffect(() => {
        localStorage.setItem('kds_eyecare', JSON.stringify(eyeCare));
    }, [eyeCare]);

    useEffect(() => {
        localStorage.setItem('kds_dimlevel', dimLevel.toString());
    }, [dimLevel]);

    useEffect(() => {
        localStorage.setItem('kds_fontsize', fontSize);
    }, [fontSize]);

    // Dynamic Font Size configurations
    const fontSizes = useMemo(() => {
        const sizeMap = {
            normal: {
                table: 'text-2xl',
                item: 'text-base',
                qty: 'text-xl',
                note: 'text-xs',
                meta: 'text-xs',
                btn: 'text-sm h-12'
            },
            large: {
                table: 'text-3xl',
                item: 'text-lg',
                qty: 'text-2xl',
                note: 'text-sm',
                meta: 'text-sm',
                btn: 'text-base h-14'
            },
            extra: {
                table: 'text-4xl',
                item: 'text-xl',
                qty: 'text-3xl',
                note: 'text-base',
                meta: 'text-base',
                btn: 'text-lg h-16'
            }
        };
        return sizeMap[fontSize] || sizeMap.normal;
    }, [fontSize]);

    // Track previously known order IDs to play chime on new orders only
    const knownOrderIds = useRef(new Set(orders.map(o => o.id)));

    useEffect(() => {
        const interval = setInterval(() => setTime(Date.now()), 10000);
        return () => clearInterval(interval);
    }, []);

    // Detect new orders and trigger sound alert
    useEffect(() => {
        const activeOrders = orders.filter(o => ['confirmed', 'preparing'].includes(o.status));
        const activeIds = activeOrders.map(o => o.id);
        const hasNewOrder = activeIds.some(id => !knownOrderIds.current.has(id));

        if (hasNewOrder) {
            // Trigger browser chime sound
            playNotificationSound();
            
            toast({
                title: "🔔 Order Mới!",
                description: "Có đơn hàng mới vừa được chuyển xuống bếp.",
                className: cn(
                    "font-bold rounded-2xl border border-transparent shadow-2xl text-white",
                    eyeCare ? "bg-amber-800 text-amber-50 border-amber-700/50 shadow-black/40" : "bg-amber-500 shadow-amber-500/20"
                ),
            });
        }

        // Update known IDs
        knownOrderIds.current = new Set(orders.map(o => o.id));
    }, [orders, toast, eyeCare]);

    const activeKitchenOrders = useMemo(() => {
        return [...orders]
            .filter(o => ['confirmed', 'preparing'].includes(o.status))
            .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
    }, [orders]);

    const filteredOrders = useMemo(() => {
        if (activeTab === 'all') return activeKitchenOrders;
        return activeKitchenOrders.filter(o => o.status === activeTab);
    }, [activeKitchenOrders, activeTab]);

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

    const getElapsedTimeInfo = (createdAt) => {
        const mins = Math.floor((time - new Date(createdAt).getTime()) / 60000);
        
        let colorClass = eyeCare 
            ? "bg-emerald-950/20 text-emerald-400/80 border-emerald-900/30"
            : "bg-emerald-500/10 text-emerald-400 border-emerald-500/20";
        let isUrgent = false;

        if (mins >= 10 && mins < 15) {
            colorClass = eyeCare
                ? "bg-amber-950/20 text-amber-400/80 border-amber-900/30"
                : "bg-amber-500/10 text-amber-400 border-amber-500/20";
        } else if (mins >= 15) {
            colorClass = eyeCare
                ? "bg-rose-950/30 text-rose-400/80 border-rose-900/40 animate-pulse font-bold"
                : "bg-rose-500/20 text-rose-400 border-rose-500/30 animate-pulse font-bold";
            isUrgent = true;
        }

        return {
            text: `${mins} phút`,
            colorClass,
            isUrgent
        };
    };

    const confirmedCount = activeKitchenOrders.filter(o => o.status === 'confirmed').length;
    const preparingCount = activeKitchenOrders.filter(o => o.status === 'preparing').length;

    return (
        <div className={cn(
            "min-h-screen p-4 md:p-8 transition-colors duration-300 relative",
            eyeCare 
                ? "bg-[#0d0c0b] text-zinc-300 selection:bg-amber-900/50" 
                : "bg-slate-950 text-slate-100"
        )}>
            {/* Ambient Eye-Care overlay for warm light shift */}
            {eyeCare && (
                <div className="fixed inset-0 pointer-events-none bg-amber-500/[0.02] mix-blend-multiply z-[9998]" />
            )}

            {/* In-app brightness dimmer */}
            {dimLevel > 0 && (
                <div 
                    className="fixed inset-0 pointer-events-none bg-black z-[9999] transition-opacity duration-200" 
                    style={{ opacity: dimLevel / 100 }} 
                />
            )}

            {/* Header Area */}
            <div className="flex flex-col xl:flex-row xl:items-center justify-between gap-4 mb-8">
                <div className="flex items-center gap-3">
                    <div className={cn(
                        "p-3 rounded-2xl shadow-md transition-colors",
                        eyeCare 
                            ? "bg-amber-600/20 text-amber-400 border border-amber-500/20" 
                            : "bg-amber-500 text-white shadow-amber-500/20"
                    )}>
                        <ChefHat className="w-8 h-8" />
                    </div>
                    <div>
                        <h1 className={cn(
                            "text-2xl md:text-3xl font-extrabold tracking-tight transition-colors",
                            eyeCare ? "text-amber-100/90" : "text-slate-100"
                        )}>
                            Bếp Điều Phối
                        </h1>
                        <p className={cn(
                            "text-sm font-medium",
                            eyeCare ? "text-zinc-500" : "text-slate-400"
                        )}>
                            Hệ thống hiển thị và chế biến thời gian thực
                        </p>
                    </div>
                </div>

                {/* Control Panel and Filtering Tabs Row */}
                <div className="flex flex-col md:flex-row md:items-center gap-4">
                    {/* Cozy Controls panel */}
                    <div className={cn(
                        "flex flex-wrap items-center gap-3 p-2 rounded-2xl border transition-colors",
                        eyeCare 
                            ? "bg-[#141210] border-[#22201d]" 
                            : "bg-slate-900 border-slate-800/85"
                    )}>
                        {/* Eye Care Toggle */}
                        <button
                            onClick={() => setEyeCare(!eyeCare)}
                            className={cn(
                                "p-2 px-3 rounded-xl flex items-center gap-2 text-xs font-bold transition-all border",
                                eyeCare 
                                    ? "bg-amber-500/10 text-amber-400 border-amber-500/20 hover:bg-amber-500/20" 
                                    : "text-slate-400 hover:text-slate-200 hover:bg-slate-800/50 border-transparent"
                            )}
                            title="Bật/Tắt bộ lọc dịu mắt"
                        >
                            {eyeCare ? <Eye className="w-4 h-4 text-amber-400" /> : <EyeOff className="w-4 h-4" />}
                            <span>Dịu mắt: {eyeCare ? "Bật" : "Tắt"}</span>
                        </button>

                        {/* Dimmer Slider */}
                        <div className={cn(
                            "flex items-center gap-2 px-3 border-l",
                            eyeCare ? "border-zinc-800" : "border-slate-800"
                        )}>
                            <Sun className={cn("w-3.5 h-3.5", eyeCare ? "text-amber-400/80" : "text-slate-400")} />
                            <input
                                type="range"
                                min="0"
                                max="60"
                                value={dimLevel}
                                onChange={(e) => setDimLevel(parseInt(e.target.value))}
                                className="w-20 md:w-24 h-1 bg-zinc-800 rounded-lg appearance-none cursor-pointer accent-amber-500"
                                title="Giảm độ chói (Dimmer)"
                            />
                            <span className="text-[10px] font-mono font-bold text-zinc-400 w-8">{dimLevel}%</span>
                        </div>

                        {/* Font Size Selector */}
                        <div className={cn(
                            "flex items-center gap-1.5 px-3 border-l",
                            eyeCare ? "border-zinc-800" : "border-slate-800"
                        )}>
                            <span className="text-xs font-bold text-zinc-500">Cỡ chữ:</span>
                            {['normal', 'large', 'extra'].map((sz) => (
                                <button
                                    key={sz}
                                    onClick={() => setFontSize(sz)}
                                    className={cn(
                                        "px-2 py-0.5 text-[11px] font-bold rounded-lg transition-all capitalize border",
                                        fontSize === sz 
                                            ? (eyeCare 
                                                ? "bg-amber-600/10 text-amber-400 border-amber-500/20" 
                                                : "bg-slate-800 text-slate-200 border-slate-700")
                                            : "text-zinc-500 hover:text-zinc-300 border-transparent"
                                    )}
                                >
                                    {sz === 'normal' ? 'vừa' : sz === 'large' ? 'lớn' : 'to'}
                                </button>
                            ))}
                        </div>
                    </div>

                    {/* Tab Filtering System */}
                    <div className={cn(
                        "flex p-1.5 rounded-2xl border transition-colors",
                        eyeCare 
                            ? "bg-[#141210] border-[#22201d]" 
                            : "bg-slate-900 border-slate-800/80"
                    )}>
                        <button
                            onClick={() => setActiveTab('all')}
                            className={cn(
                                "px-4 py-2 text-sm font-bold rounded-xl transition-all duration-200 flex items-center gap-2",
                                activeTab === 'all' 
                                    ? (eyeCare ? 'bg-amber-600/20 text-amber-400 border border-amber-500/20' : 'bg-amber-500 text-white shadow-sm') 
                                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50'
                            )}
                        >
                            Tất cả
                            <Badge className={cn(
                                "text-xs px-2 py-0.5 font-bold", 
                                activeTab === 'all' 
                                    ? (eyeCare ? 'bg-amber-600/30 text-amber-300' : 'bg-amber-600 text-white') 
                                    : (eyeCare ? 'bg-zinc-800 text-zinc-400' : 'bg-slate-800 text-slate-300')
                            )}>
                                {activeKitchenOrders.length}
                            </Badge>
                        </button>
                        <button
                            onClick={() => setActiveTab('confirmed')}
                            className={cn(
                                "px-4 py-2 text-sm font-bold rounded-xl transition-all duration-200 flex items-center gap-2",
                                activeTab === 'confirmed' 
                                    ? (eyeCare ? 'bg-blue-600/20 text-blue-400 border border-blue-500/20' : 'bg-blue-600 text-white shadow-sm') 
                                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50'
                            )}
                        >
                            Chờ chế biến
                            <Badge className={cn(
                                "text-xs px-2 py-0.5 font-bold", 
                                activeTab === 'confirmed' 
                                    ? (eyeCare ? 'bg-blue-600/30 text-blue-300' : 'bg-blue-800 text-white') 
                                    : (eyeCare ? 'bg-zinc-800 text-zinc-400' : 'bg-slate-800 text-slate-300')
                            )}>
                                {confirmedCount}
                            </Badge>
                        </button>
                        <button
                            onClick={() => setActiveTab('preparing')}
                            className={cn(
                                "px-4 py-2 text-sm font-bold rounded-xl transition-all duration-200 flex items-center gap-2",
                                activeTab === 'preparing' 
                                    ? (eyeCare ? 'bg-amber-600/20 text-amber-400 border border-amber-500/20' : 'bg-amber-600 text-white shadow-sm') 
                                    : 'text-slate-400 hover:text-slate-200 hover:bg-slate-800/50'
                            )}
                        >
                            Đang làm
                            <Badge className={cn(
                                "text-xs px-2 py-0.5 font-bold", 
                                activeTab === 'preparing' 
                                    ? (eyeCare ? 'bg-amber-700/30 text-amber-300' : 'bg-amber-700 text-white') 
                                    : (eyeCare ? 'bg-zinc-800 text-zinc-400' : 'bg-slate-800 text-slate-300')
                            )}>
                                {preparingCount}
                            </Badge>
                        </button>
                    </div>
                </div>
            </div>

            {/* Empty State */}
            {filteredOrders.length === 0 ? (
                <div className={cn(
                    "flex flex-col items-center justify-center text-center py-24 border border-dashed rounded-3xl p-8 transition-colors",
                    eyeCare 
                        ? "bg-[#141210]/40 border-[#22201d]" 
                        : "bg-slate-900/40 border-slate-800 shadow-sm"
                )}>
                    <div className={cn(
                        "p-6 rounded-full mb-4",
                        eyeCare ? "bg-[#141210] text-zinc-800" : "bg-slate-900 text-slate-700"
                    )}>
                        <Flame className="w-16 h-16 opacity-30" />
                    </div>
                    <h3 className={cn(
                        "text-xl font-bold",
                        eyeCare ? "text-zinc-400" : "text-slate-300"
                    )}>Không có đơn hàng nào</h3>
                    <p className={cn(
                        "mt-1 max-w-sm",
                        eyeCare ? "text-zinc-650" : "text-slate-500"
                    )}>
                        Hiện tại không có order nào thuộc trạng thái này cần chế biến. Hãy nghỉ ngơi chút nhé!
                    </p>
                </div>
            ) : (
                /* Responsive Grid */
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                    {filteredOrders.map(order => {
                        const elapsedInfo = getElapsedTimeInfo(order.createdAt);
                        return (
                            <div
                                key={order.id}
                                className={cn(
                                    'rounded-3xl border transition-all duration-300 flex flex-col overflow-hidden',
                                    eyeCare 
                                        ? 'bg-[#141210] border-[#22201d]/85 shadow-lg shadow-black/10 hover:shadow-black/20 hover:-translate-y-1' 
                                        : 'bg-slate-900 border-slate-800/80 shadow-lg hover:shadow-2xl hover:shadow-slate-950/50 hover:-translate-y-1',
                                    order.status === 'confirmed' 
                                        ? (eyeCare ? 'ring-1 ring-blue-500/10' : 'ring-2 ring-blue-500/10') 
                                        : (eyeCare ? 'ring-1 ring-amber-500/10' : 'ring-2 ring-amber-500/10')
                                )}
                            >
                                {/* Card Header */}
                                <div className={cn(
                                    "p-4 border-b flex flex-col gap-1.5",
                                    eyeCare 
                                        ? "bg-[#171513]/60 border-[#22201d]/65" 
                                        : "bg-slate-900/50 border-slate-800/60"
                                )}>
                                    <div className="flex justify-between items-start">
                                        <h2 className={cn(
                                            "font-extrabold tracking-tight transition-colors",
                                            fontSizes.table,
                                            eyeCare ? "text-zinc-200" : "text-slate-100"
                                        )}>
                                            Bàn {order.table?.number}
                                        </h2>
                                        <Badge className={cn(
                                            "rounded-full font-bold",
                                            fontSizes.meta,
                                            order.status === 'confirmed' 
                                                ? 'bg-blue-500/10 text-blue-400 border border-blue-500/20' 
                                                : 'bg-amber-500/10 text-amber-400 border border-amber-500/20'
                                        )}>
                                            {order.status === 'confirmed' ? 'Chờ chế biến' : 'Đang làm'}
                                        </Badge>
                                    </div>
                                    <div className={cn(
                                        "flex items-center justify-between font-semibold",
                                        fontSizes.meta,
                                        eyeCare ? "text-zinc-500" : "text-slate-400"
                                    )}>
                                        <span>Mã: {order.orderNumber}</span>
                                        <span className={cn("px-2 py-0.5 rounded-md border flex items-center gap-1 transition-colors", elapsedInfo.colorClass)}>
                                            <Clock className="w-3.5 h-3.5" />
                                            {elapsedInfo.text}
                                        </span>
                                    </div>
                                </div>

                                {/* Order Items list */}
                                <div className="p-5 flex-1 space-y-3.5">
                                    {(order.items || []).map(item => (
                                        <div 
                                            key={item.id} 
                                            className={cn(
                                                "flex flex-col gap-1 border rounded-2xl p-3 transition-colors",
                                                eyeCare 
                                                    ? "bg-[#171513]/30 border-[#22201d]/50 hover:bg-[#1a1816]/70" 
                                                    : "bg-slate-800/20 border-slate-800/60 hover:bg-slate-800/50"
                                            )}
                                        >
                                            <div className="flex justify-between items-start gap-2">
                                                <span className={cn(
                                                    "font-extrabold leading-tight transition-colors",
                                                    fontSizes.item,
                                                    eyeCare ? "text-zinc-300" : "text-slate-200"
                                                )}>
                                                    {item.menuItem?.name}
                                                </span>
                                                <span className={cn(
                                                    "font-extrabold rounded-full border flex-shrink-0 transition-colors",
                                                    fontSizes.qty,
                                                    eyeCare 
                                                        ? "bg-rose-950/20 text-rose-450 border-rose-900/30 px-3 py-0.5" 
                                                        : "text-rose-400 bg-rose-500/10 px-2.5 py-0.5 border border-rose-500/20"
                                                )}>
                                                    ×{item.quantity}
                                                </span>
                                            </div>
                                            
                                            {/* Note alert styling */}
                                            {item.note && (
                                                <div className={cn(
                                                    "border rounded-xl px-2.5 py-1.5 font-bold flex items-start gap-1.5 mt-1 animate-pulse transition-colors",
                                                    fontSizes.note,
                                                    eyeCare 
                                                        ? "bg-amber-950/20 border-amber-900/30 text-amber-400/80" 
                                                        : "bg-amber-500/10 border-amber-500/20 text-amber-300"
                                                )}>
                                                    <AlertCircle className={cn("w-4 h-4 flex-shrink-0 mt-0.5", eyeCare ? "text-amber-500/80" : "text-amber-400")} />
                                                    <span>Lưu ý: {item.note}</span>
                                                </div>
                                            )}
                                        </div>
                                    ))}
                                </div>

                                {/* Actions footer */}
                                <div className={cn(
                                    "p-4 border-t transition-colors",
                                    eyeCare 
                                        ? "bg-[#171513]/60 border-[#22201d]/65" 
                                        : "bg-slate-900/50 border-slate-800/40"
                                )}>
                                    {order.status === 'confirmed' && (
                                        <Button
                                            onClick={() => handleStatusUpdate(order.id, 'preparing')}
                                            className={cn(
                                                "w-full text-white rounded-2xl font-bold shadow-md transition-all flex items-center justify-center gap-2",
                                                fontSizes.btn,
                                                eyeCare 
                                                    ? "bg-amber-600 hover:bg-amber-700 shadow-amber-950/20" 
                                                    : "bg-amber-500 hover:bg-amber-600 shadow-amber-500/10"
                                            )}
                                        >
                                            <Play className="w-4 h-4 fill-white" /> Bắt đầu chế biến
                                        </Button>
                                    )}
                                    {order.status === 'preparing' && (
                                        <Button
                                            onClick={() => handleStatusUpdate(order.id, 'ready')}
                                            className={cn(
                                                "w-full text-white rounded-2xl font-bold shadow-md transition-all flex items-center justify-center gap-2",
                                                fontSizes.btn,
                                                eyeCare 
                                                    ? "bg-emerald-600 hover:bg-emerald-700 shadow-emerald-950/20" 
                                                    : "bg-emerald-500 hover:bg-emerald-600 shadow-emerald-500/10"
                                            )}
                                        >
                                            <Check className="w-5 h-5 stroke-[3px]" /> Xong — Sẵn sàng
                                        </Button>
                                    )}
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
}