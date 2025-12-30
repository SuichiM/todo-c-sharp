import { useState } from "react";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import {
  CssBaseline,
  AppBar,
  Toolbar,
  Typography,
  Container,
  Tabs,
  Tab,
  Box,
  Alert,
} from "@mui/material";

import { TabPanel } from "./components/TabPanel";
import { TodoItem } from "./components/TodoItem";
import { CreateTodoForm } from "./components/CreateTodoForm";

import {
  usePendingTodos,
  useCompletedTodos,
  useOverdueTodos,
} from "./hooks/useTodos";

// Create Material-UI theme
const theme = createTheme({
  palette: {
    mode: "light",
    primary: {
      main: "#1976d2",
    },
    secondary: {
      main: "#dc004e",
    },
  },
});

function App() {
  const [activeTab, setActiveTab] = useState(0);

  // Fetch todos based on active tab
  const pendingQuery = usePendingTodos();
  const completedQuery = useCompletedTodos();
  const overdueQuery = useOverdueTodos();

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setActiveTab(newValue);
  };

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box
        sx={{
          display: "flex",
          flexDirection: "column",
          minHeight: "100vh",
        }}
      >
        <AppBar position="static">
          <Toolbar>
            <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
              Todo App
            </Typography>
          </Toolbar>
        </AppBar>
        <Container
          maxWidth={false}
          sx={{
            flex: 1,
            py: 4,
            px: { xs: 2, sm: 4, md: 6 },
            maxWidth: { xs: "100%", sm: "100%", md: "90%", lg: "60%" },
            mx: "auto",
          }}
        >
          {/* Create Todo Form */}
          <CreateTodoForm />

          <Box sx={{ mt: 2, borderBottom: 1, borderColor: "divider" }}>
            <Tabs
              value={activeTab}
              onChange={handleTabChange}
              aria-label="todo tabs"
            >
              <Tab label="Pending" id="tab-0" aria-controls="tabpanel-0" />
              <Tab label="Completed" id="tab-1" aria-controls="tabpanel-1" />
              <Tab label="Overdue" id="tab-2" aria-controls="tabpanel-2" />
            </Tabs>
          </Box>

          {/* Pending Todos Tab */}
          <TabPanel
            value={activeTab}
            index={0}
            isLoading={pendingQuery.isLoading}
            isError={pendingQuery.isError}
            errorMessage={`Error loading pending todos: ${
              pendingQuery.error?.message || "Unknown error"
            }`}
          >
            {pendingQuery.isSuccess && (
              <Box>
                {pendingQuery.data.length === 0 ? (
                  <Alert severity="info">No pending todos</Alert>
                ) : (
                  pendingQuery.data.map((todo) => (
                    <TodoItem key={todo.id} todo={todo} />
                  ))
                )}
              </Box>
            )}
          </TabPanel>

          {/* Completed Todos Tab */}
          <TabPanel
            value={activeTab}
            index={1}
            isLoading={completedQuery.isLoading}
            isError={completedQuery.isError}
            errorMessage={`Error loading completed todos: ${
              completedQuery.error?.message || "Unknown error"
            }`}
          >
            {completedQuery.isSuccess && (
              <Box>
                {completedQuery.data.length === 0 ? (
                  <Alert severity="info">No completed todos</Alert>
                ) : (
                  completedQuery.data.map((todo) => (
                    <TodoItem key={todo.id} todo={todo} />
                  ))
                )}
              </Box>
            )}
          </TabPanel>

          {/* Overdue Todos Tab */}
          <TabPanel
            value={activeTab}
            index={2}
            isLoading={overdueQuery.isLoading}
            isError={overdueQuery.isError}
            errorMessage={`Error loading overdue todos: ${
              overdueQuery.error?.message || "Unknown error"
            }`}
          >
            {overdueQuery.isSuccess && (
              <Box>
                {overdueQuery.data.length === 0 ? (
                  <Alert severity="info">No overdue todos</Alert>
                ) : (
                  overdueQuery.data.map((todo) => (
                    <TodoItem key={todo.id} todo={todo} />
                  ))
                )}
              </Box>
            )}
          </TabPanel>
        </Container>
      </Box>
    </ThemeProvider>
  );
}

export default App;
