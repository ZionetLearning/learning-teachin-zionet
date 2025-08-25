import { Outlet } from "react-router-dom";
import { Box } from "@mui/material";
import { SidebarMenu } from "../";

export const SidebarMenuLayout = () => {
  return (
    <Box sx={{ display: "flex", height: "100vh" }}>
      <SidebarMenu />
      <Box sx={{ flexGrow: 1, position: "relative", overflow: "hidden" }}>
        <Outlet />
      </Box>
    </Box>
  );
};
