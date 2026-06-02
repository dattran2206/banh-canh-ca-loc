import React from "react"
import {
    AlertDialog,
    AlertDialogContent,
    AlertDialogHeader,
    AlertDialogFooter,
    AlertDialogTitle,
    AlertDialogDescription,
    AlertDialogAction,
    AlertDialogCancel,
} from "@/components/ui/alert-dialog"

export function ConfirmDialog({
    open,
    onOpenChange,
    onConfirm,
    title = "Xác nhận",
    description = "Bạn có chắc chắn muốn thực hiện hành động này không?",
    cancelText = "Hủy",
    confirmText = "Đồng ý"
}) {
    return (
        <AlertDialog open={open} onOpenChange={onOpenChange}>
            <AlertDialogContent className="w-[95%] sm:max-w-md bg-white border border-amber-100 rounded-2xl shadow-xl">
                <AlertDialogHeader className="space-y-2">
                    <AlertDialogTitle className="text-amber-900 font-bold text-lg">
                        {title}
                    </AlertDialogTitle>
                    <AlertDialogDescription className="text-amber-700/80 text-sm">
                        {description}
                    </AlertDialogDescription>
                </AlertDialogHeader>
                <AlertDialogFooter className="mt-4 gap-2">
                    <AlertDialogCancel 
                        onClick={() => onOpenChange(false)}
                        className="rounded-xl border-amber-200 text-amber-800 hover:bg-amber-50"
                    >
                        {cancelText}
                    </AlertDialogCancel>
                    <AlertDialogAction 
                        onClick={() => {
                            onConfirm();
                            onOpenChange(false);
                        }}
                        className="rounded-xl bg-amber-500 hover:bg-amber-600 text-white"
                    >
                        {confirmText}
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}
