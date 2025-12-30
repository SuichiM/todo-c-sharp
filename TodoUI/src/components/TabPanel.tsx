import { Box, CircularProgress, Alert } from "@mui/material";

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
  isLoading?: boolean;
  isError?: boolean;
  errorMessage?: string;
}

/**
 * TabPanel component to conditionally render content based on active tab
 */
export const TabPanel = ({
  children,
  value,
  index,
  isLoading,
  isError,
  errorMessage,
}: TabPanelProps) => {
  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`tabpanel-${index}`}
      aria-labelledby={`tab-${index}`}
    >
      {value === index && (
        <Box sx={{ p: 3 }}>
          {isLoading ? (
            <Box display="flex" justifyContent="center" p={3}>
              <CircularProgress />
            </Box>
          ) : isError ? (
            <Alert severity="error">
              {errorMessage || "An error occurred"}
            </Alert>
          ) : (
            children
          )}
        </Box>
      )}
    </div>
  );
};
