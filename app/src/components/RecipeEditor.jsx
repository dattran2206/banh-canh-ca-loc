import React, { useState, useEffect, useMemo } from 'react';
import { useData } from '@/lib/DataContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Plus, Trash2 } from 'lucide-react';
import apiClient from '@/api/apiClient';

export default function RecipeEditor({ menuItemId, onClose }) {
    const { menuItems, ingredients, saveRecipeItem, deleteRecipeItem } = useData();
    const [recipeItems, setRecipeItems] = useState([]);
    const [loading, setLoading] = useState(true);
    const [newIng, setNewIng] = useState({ ingredientId: '', quantity: '', yieldPercent: '100' });

    const menuItem = useMemo(() => menuItems.find(m => m.id === menuItemId), [menuItems, menuItemId]);

    const fetchRecipes = async () => {
        setLoading(true);
        try {
            const res = await apiClient.get(`/menu/recipes/${menuItemId}`);
            setRecipeItems(res.data);
        } catch (err) {
            console.error("Error fetching recipes", err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchRecipes();
    }, [menuItemId]);

    const handleAdd = async () => {
        if (!newIng.ingredientId || !newIng.quantity) return;
        try {
            await saveRecipeItem({
                menuItemId,
                ingredientId: parseInt(newIng.ingredientId),
                quantity: parseFloat(newIng.quantity),
                yieldPercent: parseFloat(newIng.yieldPercent) / 100
            });
            setNewIng({ ingredientId: '', quantity: '', yieldPercent: '100' });
            await fetchRecipes();
        } catch (err) {
            console.error("Error adding recipe item", err);
        }
    };

    const handleDelete = async (ingredientId) => {
        try {
            await deleteRecipeItem(menuItemId, ingredientId);
            await fetchRecipes();
        } catch (err) {
            console.error("Error deleting recipe item", err);
        }
    };

    const handleUpdateYield = async (ingredientId, yieldPct) => {
        const item = recipeItems.find(r => r.ingredientId === ingredientId);
        if (!item) return;
        try {
            await saveRecipeItem({
                menuItemId,
                ingredientId,
                quantity: item.quantity,
                yieldPercent: parseFloat(yieldPct) / 100
            });
            await fetchRecipes();
        } catch (err) {
            console.error("Error updating yield", err);
        }
    };

    const handleUpdateQty = async (ingredientId, qty) => {
        const item = recipeItems.find(r => r.ingredientId === ingredientId);
        if (!item) return;
        try {
            await saveRecipeItem({
                menuItemId,
                ingredientId,
                quantity: parseFloat(qty),
                yieldPercent: item.yieldPercent
            });
            await fetchRecipes();
        } catch (err) {
            console.error("Error updating qty", err);
        }
    };

    return (
        <Dialog open={true} onOpenChange={onClose}>
            <DialogContent className="w-[95%] sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle className="text-amber-900">Công thức: {menuItem?.name}</DialogTitle>
                </DialogHeader>
                <div className="space-y-3">
                    <div className="border border-amber-100 rounded-xl overflow-hidden">
                        <table className="w-full text-sm">
                            <thead className="bg-amber-50">
                                <tr>
                                    <th className="text-left px-3 py-2 text-amber-700">Nguyên liệu</th>
                                    <th className="text-left px-3 py-2 text-amber-700">SL</th>
                                    <th className="text-left px-3 py-2 text-amber-700">Yield %</th>
                                    <th className="px-3 py-2"></th>
                                </tr>
                            </thead>
                            <tbody>
                                {loading ? (
                                    <tr><td colSpan={4} className="text-center py-4 text-sm text-gray-500">Đang tải...</td></tr>
                                ) : (
                                    <>
                                        {recipeItems.map(r => {
                                            const ing = ingredients.find(i => i.id === r.ingredientId);
                                            return (
                                                <tr key={r.ingredientId} className="border-t border-amber-50">
                                                    <td className="px-3 py-2">{ing?.name} ({ing?.unit})</td>
                                                    <td className="px-3 py-2">
                                                        <Input
                                                            type="number"
                                                            defaultValue={r.quantity}
                                                            onBlur={e => handleUpdateQty(r.ingredientId, e.target.value)}
                                                            className="w-20 h-7 text-xs border-amber-200"
                                                        />
                                                    </td>
                                                    <td className="px-3 py-2">
                                                        <Input
                                                            type="number"
                                                            defaultValue={Math.round(r.yieldPercent * 100)}
                                                            onBlur={e => handleUpdateYield(r.ingredientId, e.target.value)}
                                                            className="w-20 h-7 text-xs border-amber-200"
                                                        />
                                                    </td>
                                                    <td className="px-3 py-2">
                                                        <button onClick={() => handleDelete(r.ingredientId)} className="text-red-400 hover:text-red-600">
                                                            <Trash2 className="w-4 h-4" />
                                                        </button>
                                                    </td>
                                                </tr>
                                            );
                                        })}
                                        {recipeItems.length === 0 && (
                                            <tr><td colSpan={4} className="text-center text-gray-400 py-4 text-sm">Chưa có nguyên liệu nào</td></tr>
                                        )}
                                    </>
                                )}
                            </tbody>
                        </table>
                    </div>

                    {/* Add ingredient */}
                    <div className="bg-amber-50 rounded-xl p-3 space-y-2">
                        <p className="text-sm font-medium text-amber-800">Thêm nguyên liệu</p>
                        <select
                            value={newIng.ingredientId}
                            onChange={e => setNewIng(n => ({ ...n, ingredientId: e.target.value }))}
                            className="w-full border border-amber-200 rounded-lg px-3 py-2 text-sm"
                        >
                            <option value="">-- Chọn nguyên liệu --</option>
                            {ingredients.map(i => <option key={i.id} value={i.id}>{i.name} ({i.unit})</option>)}
                        </select>
                        <div className="flex gap-2">
                            <div className="flex-1">
                                <p className="text-xs text-amber-600 mb-1">Định lượng</p>
                                <Input type="number" value={newIng.quantity} onChange={e => setNewIng(n => ({ ...n, quantity: e.target.value }))} placeholder="VD: 0.15" className="border-amber-200 h-8 text-sm" />
                            </div>
                            <div className="flex-1">
                                <p className="text-xs text-amber-600 mb-1">Yield % (mặc định 100)</p>
                                <Input type="number" value={newIng.yieldPercent} onChange={e => setNewIng(n => ({ ...n, yieldPercent: e.target.value }))} placeholder="100" className="border-amber-200 h-8 text-sm" />
                            </div>
                        </div>
                        <Button onClick={handleAdd} size="sm" className="w-full bg-amber-500 hover:bg-amber-600 text-white">
                            <Plus className="w-3 h-3 mr-1" /> Thêm
                        </Button>
                    </div>
                </div>
            </DialogContent>
        </Dialog>
    );
}