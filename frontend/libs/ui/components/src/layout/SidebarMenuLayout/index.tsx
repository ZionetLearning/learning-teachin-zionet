import { ReactNode } from "react";
import { Outlet } from "react-router-dom";
import { Box } from "@mui/material";

export interface SidebarMenuLayoutProps {
  sidebarMenu: ReactNode;
}

export const SidebarMenuLayout = ({ sidebarMenu }: SidebarMenuLayoutProps) => {
  return (
    <Box sx={{ display: "flex", height: "100vh" }}>
      {sidebarMenu}
      <Box sx={{ flexGrow: 1, position: "relative", overflow: "hidden" }}>
        <Outlet />
      </Box>
    </Box>
  );
};
