import React, { useState, useEffect } from 'react';
import { useData } from '@/lib/DataContext';
import { useAppAuth } from '@/lib/appAuth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Plus, AlertTriangle, Pencil, Trash2, PackagePlus, XCircle, ClipboardList } from 'lucide-react';
import { format } from 'date-fns';
import { useToast } from '@/components/ui/use-toast';
import apiClient from '@/api/apiClient';
import { 
    AlertDialog, 
    AlertDialogContent, 
    AlertDialogHeader, 
    AlertDialogTitle, 
    AlertDialogDescription, 
    AlertDialogFooter, 
    AlertDialogAction, 
    AlertDialogCancel 
} from '@/components/ui/alert-dialog';

export default function Inventory() {
    const { currentUser } = useAppAuth();
    const { ingredients, deleteIngredient } = useData();
    const { toast } = useToast();
    const [refresh, setRefresh] = useState(0);
    const [showAddIng, setShowAddIng] = useState(false);
    const [editIng, setEditIng] = useState(null);
    const [showStockIn, setShowStockIn] = useState(null);
    const [showWaste, setShowWaste] = useState(null);
    const [showStockTake, setShowStockTake] = useState(false);
    const [deleteConfirmIng, setDeleteConfirmIng] = useState(null);

    const [stockEntries, setStockEntries] = useState([]);
    const [wasteRecords, setWasteRecords] = useState([]);
    const [loadingEntries, setLoadingEntries] = useState(false);
    const [loadingWaste, setLoadingWaste] = useState(false);

    const fetchHistory = async () => {
        setLoadingEntries(true);
        setLoadingWaste(true);
        try {
            const [entriesRes, wasteRes] = await Promise.all([
                apiClient.get('/inventory/stock-entries'),
                apiClient.get('/inventory/waste-records')
            ]);
            setStockEntries(entriesRes.data.slice(0, 20));
            setWasteRecords(wasteRes.data.slice(0, 20));
        } catch (err) {
            console.error("Error loading inventory logs", err);
        } finally {
            setLoadingEntries(false);
            setLoadingWaste(false);
        }
    };

    useEffect(() => {
        fetchHistory();
    }, [refresh]);

    const confirmDeleteIng = async () => {
        if (!deleteConfirmIng) return;
        try {
            await deleteIngredient(deleteConfirmIng);
            setDeleteConfirmIng(null);
            toast({
                title: "Xóa nguyên liệu thành công",
                description: "Nguyên liệu đã được gỡ bỏ khỏi kho.",
            });
        } catch (err) {
            console.error("Delete ingredient failed", err);
        }
    };

    return (
        <div className="p-4 md:p-6 space-y-4">
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold text-amber-900">Kho nguyên liệu</h1>
                <div className="flex gap-2">
                    <Button onClick={() => setShowStockTake(true)} variant="outline" size="sm" className="border-amber-300 text-amber-700 text-xs">
                        <ClipboardList className="w-3 h-3 mr-1" /> Kiểm kê
                    </Button>
                    <Button onClick={() => { setEditIng(null); setShowAddIng(true); }} size="sm" className="bg-amber-500 hover:bg-amber-600 text-white rounded-xl">
                        <Plus className="w-4 h-4 mr-1" /> Thêm NL
                    </Button>
                </div>
            </div>

            <Tabs defaultValue="stock">
                <TabsList className="bg-amber-100">
                    <TabsTrigger value="stock" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Tồn kho</TabsTrigger>
                    <TabsTrigger value="in" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Lịch sử nhập</TabsTrigger>
                    <TabsTrigger value="waste" className="data-[state=active]:bg-amber-500 data-[state=active]:text-white">Phiếu hủy</TabsTrigger>
                </TabsList>

                <TabsContent value="stock" className="mt-4">
                    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                        {ingredients.map(ing => {
                            const isLow = ing.currentStock <= ing.minThreshold;
                            return (
                                <div key={ing.id} className={`border rounded-xl p-4 bg-white ${isLow ? 'border-red-300 bg-red-50' : 'border-amber-200'}`}>
                                    <div className="flex items-start justify-between mb-2">
                                        <div>
                                            <p className="font-semibold text-amber-900">{ing.name}</p>
                                            <p className="text-xs text-amber-600">Đơn vị: {ing.unit}</p>
                                        </div>
                                        {isLow && <AlertTriangle className="w-5 h-5 text-red-500 flex-shrink-0" />}
                                    </div>
                                    <div className="flex items-center justify-between mb-3">
                                        <div>
                                            <p className="text-2xl font-bold text-amber-900">{ing.currentStock}</p>
                                            <p className="text-xs text-gray-500">Tồn hiện tại ({ing.unit})</p>
                                        </div>
                                        <div className="text-right">
                                            <p className={`text-sm font-medium ${isLow ? 'text-red-600' : 'text-gray-500'}`}>{ing.minThreshold} {ing.unit}</p>
                                            <p className="text-xs text-gray-400">Ngưỡng tối thiểu</p>
                                        </div>
                                    </div>
                                    <div className="flex gap-2">
                                        <Button size="sm" onClick={() => setShowStockIn(ing)} className="flex-1 bg-green-500 hover:bg-green-600 text-white text-xs">
                                            <PackagePlus className="w-3 h-3 mr-1" /> Nhập kho
                                        </Button>
                                        <Button size="sm" onClick={() => setShowWaste(ing)} variant="outline" className="flex-1 border-red-200 text-red-600 text-xs">
                                            <XCircle className="w-3 h-3 mr-1" /> Hủy
                                        </Button>
                                        <Button size="sm" variant="outline" onClick={() => { setEditIng(ing); setShowAddIng(true); }} className="border-amber-200 text-amber-700 text-xs">
                                            <Pencil className="w-3 h-3" />
                                        </Button>
                                        <Button size="sm" variant="outline" onClick={() => setDeleteConfirmIng(ing.id)} className="border-red-200 text-red-500 text-xs">
                                            <Trash2 className="w-3.5 h-3.5" />
                                        </Button>
                                    </div>
                                </div>
                            );
                        })}
                    </div>
                </TabsContent>

                <TabsContent value="in" className="mt-4">
                    <div className="space-y-2">
                        {loadingEntries ? (
                            <p className="text-amber-600 text-center py-8">Đang tải...</p>
                        ) : (
                            <>
                                {stockEntries.length === 0 && <p className="text-amber-600 text-center py-8">Chưa có lịch sử nhập kho</p>}
                                {stockEntries.map(entry => {
                                    const ing = ingredients.find(i => i.id === entry.ingredientId);
                                    return (
                                        <div key={entry.id} className="border border-amber-100 rounded-xl px-4 py-3 bg-white flex items-center justify-between">
                                            <div>
                                                <p className="font-medium text-amber-900">{ing?.name || 'Nguyên liệu'}</p>
                                                <p className="text-xs text-gray-500">{entry.note || '—'}</p>
                                                <p className="text-xs text-gray-400">{format(new Date(entry.createdAt), 'dd/MM/yyyy HH:mm')}</p>
                                            </div>
                                            <Badge className="bg-green-100 text-green-700 text-sm">+{entry.quantity} {ing?.unit}</Badge>
                                        </div>
                                    );
                                })}
                            </>
                        )}
                    </div>
                </TabsContent>

                <TabsContent value="waste" className="mt-4">
                    <div className="flex justify-between items-center mb-3">
                        <p className="text-sm text-amber-700">Lịch sử phiếu hủy</p>
                        <Button size="sm" onClick={() => setShowWaste('new')} variant="outline" className="border-red-200 text-red-600 text-xs">
                            <Plus className="w-3 h-3 mr-1" /> Phiếu hủy mới
                        </Button>
                    </div>
                    <div className="space-y-2">
                        {loadingWaste ? (
                            <p className="text-amber-600 text-center py-8">Đang tải...</p>
                        ) : (
                            <>
                                {wasteRecords.length === 0 && <p className="text-amber-600 text-center py-8">Chưa có phiếu hủy nào</p>}
                                {wasteRecords.map(rec => {
                                    const ing = ingredients.find(i => i.id === rec.ingredientId);
                                    return (
                                        <div key={rec.id} className="border border-red-100 rounded-xl px-4 py-3 bg-white flex items-center justify-between">
                                            <div>
                                                <p className="font-medium text-red-800">{ing?.name || 'Nguyên liệu'}</p>
                                                <p className="text-xs text-gray-500">Lý do: {rec.reason}</p>
                                                <p className="text-xs text-gray-400">{format(new Date(rec.createdAt), 'dd/MM/yyyy HH:mm')}</p>
                                            </div>
                                            <Badge className="bg-red-100 text-red-700 text-sm">-{rec.quantity} {ing?.unit}</Badge>
                                        </div>
                                    );
                                })}
                            </>
                        )}
                    </div>
                </TabsContent>
            </Tabs>

            {showAddIng && (
                <IngredientForm
                    ing={editIng}
                    onClose={() => { setShowAddIng(false); setEditIng(null); }}
                    onSaved={() => { setShowAddIng(false); setEditIng(null); setRefresh(r => r + 1); }}
                />
            )}

            {showStockIn && (
                <StockInForm
                    ing={showStockIn}
                    onClose={() => setShowStockIn(null)}
                    onSaved={() => { setShowStockIn(null); setRefresh(r => r + 1); }}
                />
            )}

            {showWaste && (
                <WasteForm
                    ing={showWaste === 'new' ? null : showWaste}
                    ingredients={ingredients}
                    onClose={() => setShowWaste(null)}
                    onSaved={() => { setShowWaste(null); setRefresh(r => r + 1); }}
                />
            )}

            {showStockTake && (
                <StockTakeDialog
                    ingredients={ingredients}
                    onClose={() => setShowStockTake(false)}
                    onSaved={() => { setShowStockTake(false); setRefresh(r => r + 1); }}
                />
            )}

            {deleteConfirmIng && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setDeleteConfirmIng(null)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận xóa nguyên liệu</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn xóa nguyên liệu này khỏi hệ thống?
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={confirmDeleteIng}
                                className="bg-red-500 hover:bg-red-600 text-white"
                            >
                                Xác nhận xóa
                            </AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            )}
        </div>
    );
}

function IngredientForm({ ing, onClose, onSaved }) {
    const { addIngredient, updateIngredient } = useData();
    const [form, setForm] = useState({ name: ing?.name || '', unit: ing?.unit || 'kg', currentStock: ing?.currentStock || '', minThreshold: ing?.minThreshold || '' });
    const handleSave = async () => {
        if (!form.name) return;
        try {
            const payload = {
                name: form.name,
                unit: form.unit,
                currentStock: parseFloat(form.currentStock) || 0,
                minThreshold: parseFloat(form.minThreshold) || 0
            };
            if (ing) {
                await updateIngredient(ing.id, { ...ing, ...payload });
            } else {
                await addIngredient(payload);
            }
            onSaved();
        } catch (err) {
            console.error("Error saving ingredient", err);
        }
    };
    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader><DialogTitle className="text-amber-900">{ing ? 'Sửa nguyên liệu' : 'Thêm nguyên liệu'}</DialogTitle></DialogHeader>
                <div className="space-y-3">
                    <div><Label className="text-amber-800">Tên *</Label><Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">Đơn vị</Label><Input value={form.unit} onChange={e => setForm(f => ({ ...f, unit: e.target.value }))} className="border-amber-200 mt-1" placeholder="kg, lít, cái..." /></div>
                    <div><Label className="text-amber-800">Tồn kho hiện tại</Label><Input type="number" value={form.currentStock} onChange={e => setForm(f => ({ ...f, currentStock: e.target.value }))} className="border-amber-200 mt-1" disabled={!!ing} /></div>
                    <div><Label className="text-amber-800">Ngưỡng tối thiểu</Label><Input type="number" value={form.minThreshold} onChange={e => setForm(f => ({ ...f, minThreshold: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <Button onClick={handleSave} className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl">Lưu</Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}

function StockInForm({ ing, onClose, onSaved }) {
    const { addStockEntry } = useData();
    const [qty, setQty] = useState('');
    const [note, setNote] = useState('');
    const handleSave = async () => {
        if (!qty) return;
        try {
            await addStockEntry({
                ingredientId: ing.id,
                quantity: parseFloat(qty),
                unitPrice: 0,
                note
            });
            onSaved();
        } catch (err) {
            console.error("Error adding stock entry", err);
        }
    };
    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader><DialogTitle className="text-amber-900">Nhập kho: {ing.name}</DialogTitle></DialogHeader>
                <div className="space-y-3">
                    <div><Label className="text-amber-800">Số lượng nhập ({ing.unit})</Label><Input type="number" value={qty} onChange={e => setQty(e.target.value)} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">Ghi chú</Label><Input value={note} onChange={e => setNote(e.target.value)} className="border-amber-200 mt-1" placeholder="Lý do nhập, nguồn..." /></div>
                    <Button onClick={handleSave} className="w-full bg-green-500 hover:bg-green-600 text-white rounded-xl">Xác nhận nhập kho</Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}

function WasteForm({ ing, ingredients, onClose, onSaved }) {
    const { addWasteRecord } = useData();
    const { toast } = useToast();
    const [ingId, setIngId] = useState(ing?.id || '');
    const [qty, setQty] = useState('');
    const [reason, setReason] = useState('');
    const selectedIng = ingredients.find(i => i.id === parseInt(ingId));
    
    const handleSave = async () => {
        if (!ingId || !qty || !reason.trim()) {
            toast({
                variant: "destructive",
                title: "Thiếu thông tin",
                description: "Vui lòng điền đầy đủ thông tin!",
            });
            return;
        }
        try {
            await addWasteRecord({
                ingredientId: parseInt(ingId),
                quantity: parseFloat(qty),
                reason
            });
            onSaved();
        } catch (err) {
            console.error("Error adding waste record", err);
            toast({
                variant: "destructive",
                title: "Thao tác thất bại",
                description: err.response?.data?.message || "Không thể lưu phiếu hủy."
            });
        }
    };
    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader><DialogTitle className="text-red-700">Phiếu hủy nguyên liệu</DialogTitle></DialogHeader>
                <div className="space-y-3">
                    {!ing && <div><Label className="text-amber-800">Nguyên liệu *</Label>
                        <select value={ingId} onChange={e => setIngId(e.target.value)} className="w-full mt-1 border border-amber-200 rounded-lg px-3 py-2 text-sm">
                            <option value="">-- Chọn nguyên liệu --</option>
                            {ingredients.map(i => <option key={i.id} value={i.id}>{i.name} ({i.unit})</option>)}
                        </select></div>}
                    {ing && <p className="font-medium text-amber-900">{ing.name} — Tồn: {ing.currentStock} {ing.unit}</p>}
                    <div><Label className="text-amber-800">Số lượng hủy *</Label><Input type="number" value={qty} onChange={e => setQty(e.target.value)} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-red-700">Lý do hủy * (bắt buộc)</Label><Input value={reason} onChange={e => setReason(e.target.value)} className="border-red-200 mt-1" placeholder="Hỏng, đổ vỡ, hết hạn..." /></div>
                    <Button onClick={handleSave} className="w-full bg-red-500 hover:bg-red-600 text-white rounded-xl">Xác nhận hủy</Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}

function StockTakeDialog({ ingredients, onClose, onSaved }) {
    const { addStockTake } = useData();
    const [actuals, setActuals] = useState({});
    const [showConfirm, setShowConfirm] = useState(false);
    
    const handleConfirm = async () => {
        try {
            const promises = ingredients
                .filter(ing => actuals[ing.id] !== undefined && actuals[ing.id] !== '')
                .map(ing => {
                    return addStockTake({
                        ingredientId: ing.id,
                        actualQty: parseFloat(actuals[ing.id]),
                        note: "Kiểm kê định kỳ"
                    });
                });
            await Promise.all(promises);
            setShowConfirm(false);
            onSaved();
        } catch (err) {
            console.error("Error confirming stock take", err);
        }
    };
    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-lg max-h-[80vh] overflow-y-auto">
                <DialogHeader><DialogTitle className="text-amber-900">Kiểm kê kho</DialogTitle></DialogHeader>
                <div className="space-y-2">
                    <div className="grid grid-cols-4 text-xs font-medium text-amber-700 px-2">
                        <span>Nguyên liệu</span><span className="text-center">Lý thuyết</span><span className="text-center">Thực tế</span><span className="text-center">Lệch</span>
                    </div>
                    {ingredients.map(ing => {
                        const actual = actuals[ing.id] !== undefined && actuals[ing.id] !== '' ? parseFloat(actuals[ing.id]) : null;
                        const variance = actual !== null ? actual - ing.currentStock : null;
                        return (
                            <div key={ing.id} className="grid grid-cols-4 items-center gap-2 border border-amber-100 rounded-lg px-2 py-2">
                                <span className="text-sm text-amber-900">{ing.name} <span className="text-xs text-gray-400">({ing.unit})</span></span>
                                <span className="text-center text-sm text-gray-600">{ing.currentStock}</span>
                                <Input type="number" placeholder={String(ing.currentStock)} value={actuals[ing.id] || ''} onChange={e => setActuals(a => ({ ...a, [ing.id]: e.target.value }))} className="h-8 text-sm border-amber-200 text-center" />
                                <span className={`text-center text-sm font-medium ${variance === null ? 'text-gray-400' : variance >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                                    {variance !== null ? `${variance > 0 ? '+' : ''}${variance.toFixed(2)}` : '—'}
                                </span>
                            </div>
                        );
                    })}
                </div>
                <Button onClick={() => setShowConfirm(true)} className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl mt-2">Xác nhận kiểm kê</Button>
                
                {showConfirm && (
                    <AlertDialog open={true} onOpenChange={(open) => !open && setShowConfirm(false)}>
                        <AlertDialogContent>
                            <AlertDialogHeader>
                                <AlertDialogTitle className="text-amber-900">Xác nhận kiểm kê</AlertDialogTitle>
                                <AlertDialogDescription>
                                    Bạn có chắc chắn muốn điều chỉnh lượng tồn kho về số lượng thực tế đã nhập?
                                </AlertDialogDescription>
                            </AlertDialogHeader>
                            <AlertDialogFooter>
                                <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                                <AlertDialogAction 
                                    onClick={handleConfirm}
                                    className="bg-amber-500 hover:bg-amber-600 text-white"
                                >
                                    Xác nhận
                                </AlertDialogAction>
                            </AlertDialogFooter>
                        </AlertDialogContent>
                    </AlertDialog>
                )}
            </DialogContent>
        </Dialog>
    );
}