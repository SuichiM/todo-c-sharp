import { useState } from "react";
import {
  Card,
  CardContent,
  Typography,
  Chip,
  Box,
  TextField,
  Autocomplete,
  Checkbox,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
} from "@mui/material";
import { AccessTime, Warning, Delete as DeleteIcon } from "@mui/icons-material";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, { Dayjs } from "dayjs";
import { type TodoItem as TodoItemType, type Category } from "../types/todo";
import { useUpdateTodo, useDeleteTodo } from "../hooks/useTodoMutations";
import { useCategories } from "../hooks/useTodos";

interface TodoItemProps {
  todo: TodoItemType;
}

/**
 * TodoItem component - displays a single todo with inline editing per field
 * Each field (title, category, due time) can be edited independently
 */
export const TodoItem = ({ todo }: TodoItemProps) => {
  const [isEditingTitle, setIsEditingTitle] = useState(false);
  const [isEditingCategory, setIsEditingCategory] = useState(false);
  const [isEditingDueTime, setIsEditingDueTime] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const [editTitle, setEditTitle] = useState(todo.title);
  const [editCategory, setEditCategory] = useState<Category | null>(
    todo.category
  );
  const [editDueTime, setEditDueTime] = useState<Dayjs | null>(
    todo.dueTime ? dayjs(todo.dueTime) : null
  );

  const updateMutation = useUpdateTodo();
  const deleteMutation = useDeleteTodo();
  const { data: categories = [] } = useCategories();

  const isOverdue =
    todo.dueTime && dayjs(todo.dueTime).isBefore(dayjs()) && !todo.isCompleted;

  // Update title
  const handleSaveTitle = () => {
    if (!editTitle.trim() || editTitle === todo.title) {
      setEditTitle(todo.title);
      setIsEditingTitle(false);
      return;
    }

    if (editTitle.length < 3 || editTitle.length > 200) {
      setEditTitle(todo.title);
      setIsEditingTitle(false);
      return;
    }

    updateMutation.mutate(
      {
        id: todo.id,
        data: { title: editTitle.trim() },
      },
      {
        onSuccess: () => setIsEditingTitle(false),
        onError: () => {
          setEditTitle(todo.title);
          setIsEditingTitle(false);
        },
      }
    );
  };

  // Update category
  const handleSaveCategory = (newCategory: Category | null) => {
    if (!newCategory || newCategory.id === todo.category.id) {
      setEditCategory(todo.category);
      setIsEditingCategory(false);
      return;
    }

    updateMutation.mutate(
      {
        id: todo.id,
        data: { categoryId: newCategory.id },
      },
      {
        onSuccess: () => setIsEditingCategory(false),
        onError: () => {
          setEditCategory(todo.category);
          setIsEditingCategory(false);
        },
      }
    );
  };

  // Update due time
  const handleSaveDueTime = (newDueTime: Dayjs | null) => {
    if (newDueTime && newDueTime.isBefore(dayjs())) {
      setEditDueTime(todo.dueTime ? dayjs(todo.dueTime) : null);
      setIsEditingDueTime(false);
      return;
    }

    const newValue = newDueTime ? newDueTime.toISOString() : null;
    const currentValue = todo.dueTime;

    if (newValue === currentValue) {
      setIsEditingDueTime(false);
      return;
    }

    updateMutation.mutate(
      {
        id: todo.id,
        data: { dueTime: newValue },
      },
      {
        onSuccess: () => setIsEditingDueTime(false),
        onError: () => {
          setEditDueTime(todo.dueTime ? dayjs(todo.dueTime) : null);
          setIsEditingDueTime(false);
        },
      }
    );
  };

  // Toggle completion status
  const handleToggleCompletion = () => {
    updateMutation.mutate({
      id: todo.id,
      data: { isCompleted: !todo.isCompleted },
    });
  };

  // Delete todo
  const handleDeleteClick = () => {
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = () => {
    deleteMutation.mutate(todo.id, {
      onSuccess: () => setDeleteDialogOpen(false),
    });
  };

  const handleDeleteCancel = () => {
    setDeleteDialogOpen(false);
  };

  return (
    <>
      <Card
        sx={{
          mb: 2,
          borderLeft: isOverdue
            ? "4px solid #d32f2f"
            : todo.isCompleted
            ? "4px solid #2e7d32"
            : undefined,
          bgcolor: isOverdue ? "#ffebee" : undefined,
        }}
      >
        <CardContent>
          <Box
            display="flex"
            justifyContent="space-between"
            alignItems="flex-start"
            mb={1}
            gap={1}
          >
            {/* Completion Checkbox */}
            <Checkbox
              checked={todo.isCompleted}
              onChange={handleToggleCompletion}
              disabled={updateMutation.isPending}
              sx={{ mt: -1, ml: -1 }}
            />

            {/* Title - Editable inline */}
            {isEditingTitle && !todo.isCompleted ? (
              <TextField
                value={editTitle}
                onChange={(e) => setEditTitle(e.target.value)}
                onBlur={handleSaveTitle}
                onKeyDown={(e) => {
                  if (e.key === "Enter") handleSaveTitle();
                  if (e.key === "Escape") {
                    setEditTitle(todo.title);
                    setIsEditingTitle(false);
                  }
                }}
                autoFocus
                fullWidth
                size="small"
                variant="standard"
                disabled={updateMutation.isPending}
              />
            ) : (
              <Box display="flex" alignItems="center" gap={1} flex={1}>
                <Typography
                  variant="h6"
                  component="div"
                  sx={{
                    flex: 1,
                    cursor: !todo.isCompleted ? "pointer" : "default",
                    textDecoration: todo.isCompleted ? "line-through" : "none",
                    color: todo.isCompleted ? "text.secondary" : "text.primary",
                    "&:hover": !todo.isCompleted
                      ? { bgcolor: "action.hover", borderRadius: 1 }
                      : {},
                  }}
                  onClick={() => !todo.isCompleted && setIsEditingTitle(true)}
                >
                  {todo.title}
                </Typography>
              </Box>
            )}

            {/* Category - Editable as dropdown */}
            {isEditingCategory && !todo.isCompleted ? (
              <Autocomplete
                options={categories}
                getOptionLabel={(option) => option.name}
                value={editCategory}
                onChange={(_event, newValue) => handleSaveCategory(newValue)}
                onBlur={() => {
                  setEditCategory(todo.category);
                  setIsEditingCategory(false);
                }}
                open={isEditingCategory}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    size="small"
                    variant="standard"
                    autoFocus
                  />
                )}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                sx={{ minWidth: 150 }}
                disabled={updateMutation.isPending}
              />
            ) : (
              <Chip
                label={todo.category.name}
                size="small"
                color="primary"
                variant="outlined"
                onClick={() => !todo.isCompleted && setIsEditingCategory(true)}
                sx={{
                  cursor: !todo.isCompleted ? "pointer" : "default",
                }}
              />
            )}
          </Box>

          {/* Dates section */}
          <Box
            display="flex"
            gap={2}
            alignItems="center"
            flexWrap="wrap"
            justifyContent="space-between"
          >
            <Box display="flex" gap={2} alignItems="center" flexWrap="wrap">
              <Box display="flex" alignItems="center" gap={0.5}>
                <AccessTime fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  Created: {dayjs(todo.createdAt).format("MMM D, YYYY")}
                </Typography>
              </Box>

              {/* Due Time - Editable with date picker */}
              {isEditingDueTime && !todo.isCompleted ? (
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                  <DateTimePicker
                    value={editDueTime}
                    onChange={(newValue) => {
                      setEditDueTime(newValue);
                    }}
                    onAccept={(newValue) => handleSaveDueTime(newValue)}
                    onClose={() => {
                      setEditDueTime(todo.dueTime ? dayjs(todo.dueTime) : null);
                      setIsEditingDueTime(false);
                    }}
                    open={isEditingDueTime}
                    minDateTime={dayjs()}
                    slotProps={{
                      textField: {
                        size: "small",
                        variant: "standard",
                        sx: { minWidth: 200 },
                      },
                    }}
                    disabled={updateMutation.isPending}
                  />
                </LocalizationProvider>
              ) : (
                todo.dueTime && (
                  <Box
                    display="flex"
                    alignItems="center"
                    gap={0.5}
                    sx={{
                      cursor: !todo.isCompleted ? "pointer" : "default",
                      "&:hover": !todo.isCompleted
                        ? { bgcolor: "action.hover", borderRadius: 1, px: 1 }
                        : {},
                    }}
                    onClick={() =>
                      !todo.isCompleted && setIsEditingDueTime(true)
                    }
                  >
                    {isOverdue && <Warning fontSize="small" color="error" />}
                    <Typography
                      variant="body2"
                      color={isOverdue ? "error" : "text.secondary"}
                      fontWeight={isOverdue ? 600 : 400}
                    >
                      Due: {dayjs(todo.dueTime).format("MMM D, YYYY h:mm A")}
                      {isOverdue && " (Overdue)"}
                    </Typography>
                  </Box>
                )
              )}

              {/* Add due time if not set */}
              {!todo.dueTime && !todo.isCompleted && !isEditingDueTime && (
                <Chip
                  label="+ Add due time"
                  size="small"
                  variant="outlined"
                  onClick={() => setIsEditingDueTime(true)}
                  sx={{ cursor: "pointer" }}
                />
              )}
            </Box>

            {/* Delete button aligned to the right */}
            <IconButton
              onClick={handleDeleteClick}
              size="small"
              color="error"
              disabled={deleteMutation.isPending}
            >
              <DeleteIcon fontSize="small" />
            </IconButton>
          </Box>
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog
        open={deleteDialogOpen}
        onClose={handleDeleteCancel}
        aria-labelledby="delete-dialog-title"
        aria-describedby="delete-dialog-description"
      >
        <DialogTitle id="delete-dialog-title">Delete Todo?</DialogTitle>
        <DialogContent>
          <DialogContentText id="delete-dialog-description">
            Are you sure you want to delete "{todo.title}"? This action cannot
            be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={handleDeleteCancel}
            disabled={deleteMutation.isPending}
          >
            Cancel
          </Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleteMutation.isPending}
            autoFocus
          >
            {deleteMutation.isPending ? "Deleting..." : "Delete"}
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};
