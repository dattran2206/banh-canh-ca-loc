import React, { useState, useMemo } from 'react';
import { getList, setList, addToList, removeFromList, generateId, KEYS } from '@/lib/storage';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Switch } from '@/components/ui/switch';
import { Plus, Pencil, Trash2, ChefHat } from 'lucide-react';
import { useAppAuth } from '@/lib/appAuth';
import { doLogActivity } from '@/lib/appAuth';
import RecipeEditor from '@/components/RecipeEditor';
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

export default function Menu() {
    const { currentUser } = useAppAuth();
    const [refresh, setRefresh] = useState(0);
    const [editItem, setEditItem] = useState(null);
    const [showForm, setShowForm] = useState(false);
    const [recipeItemId, setRecipeItemId] = useState(null);
    const [activeCategory, setActiveCategory] = useState('all');
    const [showCatForm, setShowCatForm] = useState(false);
    const [editCat, setEditCat] = useState(null);
    const [deleteConfirmItem, setDeleteConfirmItem] = useState(null);
    const [deleteConfirmCat, setDeleteConfirmCat] = useState(null);

    const { categories, menuItems } = useMemo(() => {
        return { categories: getList(KEYS.CATEGORIES), menuItems: getList(KEYS.MENU_ITEMS) };
    }, [refresh]);

    const filtered = activeCategory === 'all' ? menuItems : menuItems.filter(m => m.categoryId === activeCategory);

    const handleToggleAvailable = (item) => {
        const items = getList(KEYS.MENU_ITEMS);
        const idx = items.findIndex(i => i.id === item.id);
        if (idx !== -1) {
            items[idx] = { ...items[idx], isAvailable: !items[idx].isAvailable };
            setList(KEYS.MENU_ITEMS, items);
            setRefresh(r => r + 1);
        }
    };

    const confirmDeleteItem = () => {
        if (!deleteConfirmItem) return;
        removeFromList(KEYS.MENU_ITEMS, deleteConfirmItem);
        doLogActivity(currentUser?.id, 'delete_menu_item', `Xóa món ăn`);
        setDeleteConfirmItem(null);
        setRefresh(r => r + 1);
    };

    const confirmDeleteCategory = () => {
        if (!deleteConfirmCat) return;
        removeFromList(KEYS.CATEGORIES, deleteConfirmCat);
        setDeleteConfirmCat(null);
        setRefresh(r => r + 1);
    };

    return (
        <div className="p-4 md:p-6 space-y-4">
            <div className="flex items-center justify-between">
                <h1 className="text-2xl font-bold text-amber-900">Thực đơn</h1>
                <div className="flex gap-2">
                    <Button onClick={() => { setEditCat(null); setShowCatForm(true); }} variant="outline" size="sm" className="border-amber-300 text-amber-700">
                        <Plus className="w-3 h-3 mr-1" /> Danh mục
                    </Button>
                    <Button onClick={() => { setEditItem(null); setShowForm(true); }} size="sm" className="bg-amber-500 hover:bg-amber-600 text-white rounded-xl">
                        <Plus className="w-4 h-4 mr-1" /> Thêm món
                    </Button>
                </div>
            </div>

            {/* Category tabs */}
            <div className="flex flex-wrap gap-2">
                <button
                    onClick={() => setActiveCategory('all')}
                    className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${activeCategory === 'all' ? 'bg-amber-500 text-white' : 'bg-amber-100 text-amber-700 hover:bg-amber-200'}`}
                >
                    Tất cả ({menuItems.length})
                </button>
                {categories.map(cat => (
                    <div key={cat.id} className="flex items-center gap-1">
                        <button
                            onClick={() => setActiveCategory(cat.id)}
                            className={`px-3 py-1.5 rounded-full text-sm font-medium transition-colors ${activeCategory === cat.id ? 'bg-amber-500 text-white' : 'bg-amber-100 text-amber-700 hover:bg-amber-200'}`}
                        >
                            {cat.name} ({menuItems.filter(m => m.categoryId === cat.id).length})
                        </button>
                        <button onClick={() => { setEditCat(cat); setShowCatForm(true); }} className="text-amber-400 hover:text-amber-600">
                            <Pencil className="w-3 h-3" />
                        </button>
                        <button onClick={() => setDeleteConfirmCat(cat.id)} className="text-red-400 hover:text-red-600">
                            <Trash2 className="w-3 h-3" />
                        </button>
                    </div>
                ))}
            </div>

            {/* Menu items grid */}
            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {filtered.map(item => {
                    const cat = categories.find(c => c.id === item.categoryId);
                    return (
                        <div key={item.id} className="border border-amber-200 rounded-xl p-4 bg-white">
                            <div className="flex items-start justify-between mb-2">
                                <div className="flex-1">
                                    <p className="font-semibold text-amber-900">{item.name}</p>
                                    {cat && <Badge variant="outline" className="text-xs border-amber-200 text-amber-600 mt-1">{cat.name}</Badge>}
                                </div>
                                <Switch checked={item.isAvailable} onCheckedChange={() => handleToggleAvailable(item)} />
                            </div>
                            {item.description && <p className="text-xs text-gray-500 mb-2">{item.description}</p>}
                            <p className="text-amber-600 font-bold">{item.price.toLocaleString('vi-VN')}đ</p>
                            <div className="flex gap-2 mt-3">
                                <Button size="sm" variant="outline" onClick={() => { setEditItem(item); setShowForm(true); }} className="flex-1 border-amber-200 text-amber-700 text-xs">
                                    <Pencil className="w-3 h-3 mr-1" /> Sửa
                                </Button>
                                <Button size="sm" variant="outline" onClick={() => setRecipeItemId(item.id)} className="flex-1 border-amber-200 text-amber-700 text-xs">
                                    <ChefHat className="w-3 h-3 mr-1" /> Công thức
                                </Button>
                                <Button size="sm" variant="outline" onClick={() => setDeleteConfirmItem(item.id)} className="border-red-200 text-red-500 hover:bg-red-50 text-xs">
                                    <Trash2 className="w-3 h-3" />
                                </Button>
                            </div>
                        </div>
                    );
                })}
            </div>

            {showForm && (
                <MenuItemForm
                    item={editItem}
                    categories={categories}
                    onClose={() => { setShowForm(false); setEditItem(null); }}
                    onSaved={() => { setShowForm(false); setEditItem(null); setRefresh(r => r + 1); }}
                />
            )}

            {showCatForm && (
                <CategoryForm
                    cat={editCat}
                    onClose={() => { setShowCatForm(false); setEditCat(null); }}
                    onSaved={() => { setShowCatForm(false); setEditCat(null); setRefresh(r => r + 1); }}
                />
            )}

            {recipeItemId && (
                <RecipeEditor menuItemId={recipeItemId} onClose={() => setRecipeItemId(null)} />
            )}

            {deleteConfirmItem && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setDeleteConfirmItem(null)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận xóa món ăn</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn xóa món ăn này khỏi thực đơn?
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={confirmDeleteItem}
                                className="bg-red-500 hover:bg-red-600 text-white"
                            >
                                Xác nhận xóa
                            </AlertDialogAction>
                        </AlertDialogFooter>
                    </AlertDialogContent>
                </AlertDialog>
            )}

            {deleteConfirmCat && (
                <AlertDialog open={true} onOpenChange={(open) => !open && setDeleteConfirmCat(null)}>
                    <AlertDialogContent>
                        <AlertDialogHeader>
                            <AlertDialogTitle className="text-amber-900">Xác nhận xóa danh mục</AlertDialogTitle>
                            <AlertDialogDescription>
                                Bạn có chắc chắn muốn xóa danh mục này? Tất cả món ăn thuộc danh mục sẽ không còn danh mục phân loại.
                            </AlertDialogDescription>
                        </AlertDialogHeader>
                        <AlertDialogFooter>
                            <AlertDialogCancel className="border-amber-200 text-amber-800">Hủy</AlertDialogCancel>
                            <AlertDialogAction 
                                onClick={confirmDeleteCategory}
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

function MenuItemForm({ item, categories, onClose, onSaved }) {
    const { currentUser } = useAppAuth();
    const [form, setForm] = useState({
        name: item?.name || '',
        categoryId: item?.categoryId || '',
        price: item?.price || '',
        description: item?.description || '',
        isAvailable: item?.isAvailable !== false,
    });

    const handleSave = () => {
        if (!form.name || !form.price) return;
        if (item) {
            const items = getList(KEYS.MENU_ITEMS);
            const idx = items.findIndex(i => i.id === item.id);
            if (idx !== -1) {
                items[idx] = { ...items[idx], ...form, price: parseFloat(form.price) };
                setList(KEYS.MENU_ITEMS, items);
            }
            doLogActivity(currentUser?.id, 'edit_menu_item', `Sửa món ${form.name}`);
        } else {
            addToList(KEYS.MENU_ITEMS, { id: generateId(), ...form, price: parseFloat(form.price) });
            doLogActivity(currentUser?.id, 'add_menu_item', `Thêm món ${form.name}`);
        }
        onSaved();
    };

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-sm">
                <DialogHeader><DialogTitle className="text-amber-900">{item ? 'Sửa món' : 'Thêm món mới'}</DialogTitle></DialogHeader>
                <div className="space-y-3">
                    <div><Label className="text-amber-800">Tên món *</Label>
                        <Input value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">Danh mục</Label>
                        <select value={form.categoryId} onChange={e => setForm(f => ({ ...f, categoryId: e.target.value }))} className="w-full mt-1 border border-amber-200 rounded-lg px-3 py-2 text-sm">
                            <option value="">-- Chọn danh mục --</option>
                            {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                        </select></div>
                    <div><Label className="text-amber-800">Giá (đ) *</Label>
                        <Input type="number" value={form.price} onChange={e => setForm(f => ({ ...f, price: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <div><Label className="text-amber-800">Mô tả</Label>
                        <Input value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} className="border-amber-200 mt-1" /></div>
                    <Button onClick={handleSave} className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl">Lưu</Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}

function CategoryForm({ cat, onClose, onSaved }) {
    const [name, setName] = useState(cat?.name || '');
    const handleSave = () => {
        if (!name.trim()) return;
        if (cat) {
            const cats = getList(KEYS.CATEGORIES);
            const idx = cats.findIndex(c => c.id === cat.id);
            if (idx !== -1) { cats[idx] = { ...cats[idx], name }; setList(KEYS.CATEGORIES, cats); }
        } else {
            addToList(KEYS.CATEGORIES, { id: generateId(), name });
        }
        onSaved();
    };
    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="max-w-xs">
                <DialogHeader><DialogTitle className="text-amber-900">{cat ? 'Sửa danh mục' : 'Thêm danh mục'}</DialogTitle></DialogHeader>
                <div className="space-y-3">
                    <div><Label className="text-amber-800">Tên danh mục</Label>
                        <Input value={name} onChange={e => setName(e.target.value)} className="border-amber-200 mt-1" /></div>
                    <Button onClick={handleSave} className="w-full bg-amber-500 hover:bg-amber-600 text-white rounded-xl">Lưu</Button>
                </div>
            </DialogContent>
        </Dialog>
    );
}