import { useState } from "react";
import {
  Box,
  Paper,
  TextField,
  Button,
  Autocomplete,
  IconButton,
} from "@mui/material";
import { Close as CloseIcon } from "@mui/icons-material";
import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, { Dayjs } from "dayjs";
import { useCreateTodo } from "../hooks/useTodoMutations";
import { useCategories } from "../hooks/useTodos";
import { type Category } from "../types/todo";

/**
 * Form component for creating a new todo item
 * Extracts categories from existing todos for the dropdown
 */
export const CreateTodoForm = () => {
  const [title, setTitle] = useState("");
  const [category, setCategory] = useState<Category | null>(null);
  const [dueTime, setDueTime] = useState<Dayjs | null>(null);
  const [formError, setFormError] = useState<string>("");
  const [isExpanded, setIsExpanded] = useState(false);

  const createMutation = useCreateTodo();
  const { data: categories = [], isLoading: categoriesLoading } =
    useCategories();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setFormError("");

    // Validation
    if (!title.trim()) {
      setFormError("Title is required");
      return;
    }
    if (title.length < 3 || title.length > 200) {
      setFormError("Title must be between 3 and 200 characters");
      return;
    }
    if (!category) {
      setFormError("Category is required");
      return;
    }
    if (dueTime && dueTime.isBefore(dayjs())) {
      setFormError("Due time must be in the future");
      return;
    }

    // Submit
    createMutation.mutate(
      {
        title: title.trim(),
        categoryId: category.id,
        dueTime: dueTime ? dueTime.toISOString() : undefined,
      },
      {
        onSuccess: () => {
          // Reset form
          setTitle("");
          setCategory(null);
          setDueTime(null);
          setIsExpanded(false);
        },
        onError: (error: unknown) => {
          const err = error as {
            response?: { data?: { message?: string } };
            message?: string;
          };
          setFormError(
            err.response?.data?.message ||
              err.message ||
              "Failed to create todo"
          );
        },
      }
    );
  };

  const handleReset = () => {
    setTitle("");
    setCategory(null);
    setDueTime(null);
    setFormError("");
    setIsExpanded(false);
  };

  const handleTitleFocus = () => {
    setIsExpanded(true);
  };

  return (
    <Paper elevation={2} sx={{ p: 2 }}>
      <form onSubmit={handleSubmit}>
        <Box display="flex" flexDirection="column" gap={2}>
          {/* First Row: Title input with optional close button */}
          <Box display="flex" gap={1} alignItems="center">
            {isExpanded && (
              <IconButton
                onClick={handleReset}
                size="small"
                aria-label="close"
                sx={{ flexShrink: 0 }}
              >
                <CloseIcon />
              </IconButton>
            )}
            <TextField
              label={isExpanded ? "Title" : "New todo..."}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              onFocus={handleTitleFocus}
              required
              fullWidth
              placeholder="Title"
              error={!!formError && !title.trim()}
              size="small"
            />
          </Box>

          {/* Second Row: Category, Due Time, and Submit Button (conditionally rendered) */}
          {isExpanded && (
            <Box
              display="flex"
              flexDirection={{ xs: "column", sm: "row" }}
              gap={2}
              alignItems={{ xs: "stretch", sm: "flex-start" }}
            >
              <Autocomplete
                options={categories}
                getOptionLabel={(option) => option.name}
                value={category}
                onChange={(_event, newValue) => setCategory(newValue)}
                loading={categoriesLoading}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Category"
                    required
                    error={!!formError && !category}
                    size="small"
                  />
                )}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                sx={{ flex: 1, minWidth: { xs: "auto", sm: 150 } }}
              />

              <LocalizationProvider dateAdapter={AdapterDayjs}>
                <DateTimePicker
                  label="Due Time (Optional)"
                  value={dueTime}
                  onChange={(newValue) => setDueTime(newValue)}
                  minDateTime={dayjs()}
                  slotProps={{
                    textField: {
                      size: "small",
                      error:
                        !!formError &&
                        dueTime !== null &&
                        dueTime.isBefore(dayjs()),
                      sx: { flex: 1, minWidth: { xs: "auto", sm: 200 } },
                    },
                  }}
                />
              </LocalizationProvider>

              <Button
                type="submit"
                variant="contained"
                disabled={createMutation.isPending}
                sx={{
                  minWidth: { xs: "100%", sm: 120 },
                  height: 40,
                }}
                size="medium"
              >
                {createMutation.isPending ? "Creating..." : "Create"}
              </Button>
            </Box>
          )}
        </Box>
      </form>
    </Paper>
  );
};
