import { ReactNode } from "react";
import { Outlet } from "react-router-dom";
import { Box, Container } from "@mui/material";
import { useStyles } from "./style";

export interface SidebarMenuLayoutProps {
  sidebarMenu: ReactNode;
}

export const SidebarMenuLayout = ({ sidebarMenu }: SidebarMenuLayoutProps) => {
  const classes = useStyles();

  return (
    <Box className={classes.layout}>
      {sidebarMenu}

      <Box className={classes.content}>
        <Container>
          <Outlet />
        </Container>
      </Box>
    </Box>
  );
};
