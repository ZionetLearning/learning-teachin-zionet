import { useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Sidebar,
  Menu,
  MenuItem,
  SubMenu,
  sidebarClasses,
} from "react-pro-sidebar";

import MenuIcon from "@mui/icons-material/Menu";
import ChatIcon from "@mui/icons-material/Chat";
import FaceIcon from "@mui/icons-material/Face";
import KeyboardIcon from "@mui/icons-material/Keyboard";
import HomeIcon from "@mui/icons-material/Home";

export const SidebarMenu = () => {
  const navigate = useNavigate();
  const [collapsed, setCollapsed] = useState(false);

  const handleNavigation = (path: string) => {
    navigate(path);
  };

  return (
    <Sidebar
      collapsed={collapsed}
      rootStyles={{
        [`.${sidebarClasses.container}`]: {
          backgroundColor: "#f4f4f4",
          borderRight: "1px solid #ddd",
          height: "100vh",
        },
      }}
    >
      <Menu>
        <MenuItem
          icon={<MenuIcon />}
          onClick={() => setCollapsed((prev) => !prev)}
        >
          {!collapsed && "Toggle Sidebar"}
        </MenuItem>

        <MenuItem icon={<HomeIcon />} onClick={() => handleNavigation("/")}>
          Home
        </MenuItem>

        <SubMenu label="Chat Tools" icon={<ChatIcon />}>
          <MenuItem onClick={() => handleNavigation("/chat/sh")}>
            Chat - Sh (Shirley)
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/chat/yo")}>
            Chat - Yo (Yonatan)
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/chat/da")}>
            Chat - Da (Daniel)
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/chat/ou")}>
            Chat - Ou (Ouriel)
          </MenuItem>
        </SubMenu>

        <SubMenu label="Avatar Tools" icon={<FaceIcon />}>
          <MenuItem onClick={() => handleNavigation("/avatar/ou")}>
            Avatar - Ou
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/avatar/sh")}>
            Avatar - Sh
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/avatar/da")}>
            Avatar - Da
          </MenuItem>
        </SubMenu>

        <SubMenu label="Practice Tools" icon={<KeyboardIcon />}>
          <MenuItem onClick={() => handleNavigation("/typing")}>
            Typing Practice
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/word-order-game")}>
            Word Order Game
          </MenuItem>
          <MenuItem onClick={() => handleNavigation("/speaking")}>
            Speaking Practice
          </MenuItem>
        </SubMenu>
      </Menu>
    </Sidebar>
  );
};
