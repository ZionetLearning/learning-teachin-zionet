import { Box, Typography } from "@mui/material";
import { SidebarMenu } from "../components";

export const HomePage = () => {
  return (
    <Box sx={{ display: "flex", height: "100%" }}>
      <SidebarMenu />

      <Box sx={{ flexGrow: 1, padding: 3 }}>
        <Typography variant="h4" gutterBottom>
          Welcome to our internal playground project
        </Typography>
        <Typography>Select a tool from the sidebar to begin.</Typography>
      </Box>
    </Box>
  );
};
