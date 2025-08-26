import { useState } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
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
import TranslateIcon from "@mui/icons-material/Translate";
import HomeIcon from "@mui/icons-material/Home";
import ExitToAppIcon from "@mui/icons-material/ExitToApp";
import PublicIcon from "@mui/icons-material/Public";
import WeatherWidgetIcon from "@mui/icons-material/Cloud";
import ThreePIcon from "@mui/icons-material/ThreeP";
import LiveTvIcon from "@mui/icons-material/LiveTv";
import ConnectWithoutContactIcon from "@mui/icons-material/ConnectWithoutContact";
import FlagIcon from "@mui/icons-material/Flag";
import GBFlag from "country-flag-icons/react/3x2/GB";
import ILFlag from "country-flag-icons/react/3x2/IL";
import { useAuth } from "@/providers/auth";

export const SidebarMenu = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout } = useAuth();
  const { t, i18n } = useTranslation();
  const [collapsed, setCollapsed] = useState(false);
  const flagSize = { width: 22, height: 16 };
  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const isHebrew = i18n.language === "he";
  const isActive = (path: string) => location.pathname === path;

  const handleChangeLanguage = (lng: "en" | "he") => () => {
    i18n.changeLanguage(lng);
  };
  return (
    <Sidebar
      data-testid="app-sidebar"
      collapsed={collapsed}
      dir={isHebrew ? "rtl" : "ltr"}
      rootStyles={{
        [`.${sidebarClasses.container}`]: {
          backgroundColor: "#f4f4f4",
          borderRight: "1px solid #ddd",
          height: "100vh",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          direction: isHebrew ? "rtl" : "ltr",
        },
      }}
    >
      <Menu
        menuItemStyles={{
          button: ({ active }) => ({
            color: active ? "white" : "#333",
            backgroundColor: active ? "#7c4dff" : "transparent",
            borderRadius: "8px",
            margin: "4px 8px",
            padding: "10px",
            "& .ps-menu-icon": {
              color: active ? "#fff" : "#7c4dff",
            },
            "&:hover": {
              backgroundColor: active ? "#6a40e6" : "#f0f0f0",
              color: active ? "#fff" : "#000",
            },
            textTransform: "capitalize",
          }),
          label: {
            textAlign: isHebrew ? "right" : "left",
          },
        }}
      >
        <MenuItem
          icon={<MenuIcon />}
          onClick={() => setCollapsed((prev) => !prev)}
        >
          {!collapsed && t("sidebar.toggleSidebar")}
        </MenuItem>

        <SubMenu
          label={t("sidebar.languages")}
          icon={<TranslateIcon />}
          data-testid="sidebar-languages"
        >
          <MenuItem
            icon={<ILFlag style={flagSize} />}
            onClick={handleChangeLanguage("he")}
            active={i18n.language === "he"}
            data-testid="sidebar-lang-he"
          >
            {t("sidebar.he")}
          </MenuItem>

          <MenuItem
            icon={<GBFlag style={flagSize} />}
            onClick={handleChangeLanguage("en")}
            active={i18n.language === "en"}
            data-testid="sidebar-lang-en"
          >
            {t("sidebar.en")}
          </MenuItem>
        </SubMenu>

        <MenuItem
          icon={<HomeIcon />}
          onClick={() => handleNavigation("/")}
          active={isActive("/")}
          data-testid="sidebar-home"
        >
          {t("sidebar.home")}
        </MenuItem>

        <MenuItem
          icon={<ConnectWithoutContactIcon />}
          onClick={() => handleNavigation("/signalr")}
          active={isActive("/signalr")}
          data-testid="signalR"
        >
          {t("sidebar.signalR")}
        </MenuItem>

        <MenuItem
          icon={<ThreePIcon />}
          onClick={() => handleNavigation("/chat-with-avatar")}
          active={isActive("/chat-with-avatar")}
          data-testid="sidebar-chat-avatar"
        >
          {t("sidebar.chatAvatar")}
        </MenuItem>

        <SubMenu label={t("sidebar.chatTools")} icon={<ChatIcon />}>
          <MenuItem
            onClick={() => handleNavigation("/chat/yo")}
            active={isActive("/chat/yo")}
            data-testid="sidebar-chat-yo"
          >
            {t("sidebar.chatYo")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/chat/da")}
            active={isActive("/chat/da")}
            data-testid="sidebar-chat-da"
          >
            {t("sidebar.chatDa")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/chat/ou")}
            active={isActive("/chat/ou")}
            data-testid="sidebar-chat-ou"
          >
            {t("sidebar.chatOu")}
          </MenuItem>
        </SubMenu>

        <SubMenu label={t("sidebar.avatarTools")} icon={<FaceIcon />}>
          <MenuItem
            onClick={() => handleNavigation("/avatar/ou")}
            active={isActive("/avatar/ou")}
            data-testid="sidebar-avatar-ou"
          >
            {t("sidebar.avatarOu")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/avatar/sh")}
            active={isActive("/avatar/sh")}
            data-testid="sidebar-avatar-sh"
          >
            {t("sidebar.avatarSh")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/avatar/da")}
            active={isActive("/avatar/da")}
            data-testid="sidebar-avatar-da"
          >
            {t("sidebar.avatarDa")}
          </MenuItem>
        </SubMenu>

        <SubMenu label={t("sidebar.practiceTools")} icon={<KeyboardIcon />}>
          <MenuItem
            onClick={() => handleNavigation("/typing")}
            active={isActive("/typing")}
            data-testid="sidebar-typing"
          >
            {t("sidebar.typingPractice")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/word-order-game")}
            active={isActive("/word-order-game")}
            data-testid="sidebar-word-order"
          >
            {t("sidebar.wordOrderGame")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/speaking")}
            active={isActive("/speaking")}
            data-testid="sidebar-speaking"
          >
            {t("sidebar.speakingPractice")}
          </MenuItem>
        </SubMenu>

        <MenuItem
          icon={<PublicIcon />}
          onClick={() => handleNavigation("/earthquake-map")}
          active={isActive("/earthquake-map")}
          data-testid="sidebar-earthquake"
        >
          {t("sidebar.earthquakeMap")}
        </MenuItem>
        <MenuItem
          onClick={() => handleNavigation("/weather")}
          icon={<WeatherWidgetIcon />}
          active={isActive("/weather")}
          data-testid="sidebar-weather"
        >
          {t("sidebar.weather")}
        </MenuItem>
        <MenuItem
          onClick={() => handleNavigation("/anime-explorer")}
          icon={<LiveTvIcon />}
          active={isActive("/anime-explorer")}
          data-testid="sidebar-anime"
        >
          {t("sidebar.anime")}
        </MenuItem>
        <MenuItem
          onClick={() => handleNavigation("/country-explorer")}
          icon={<FlagIcon />}
          active={isActive("/country-explorer")}
        >
          {t("sidebar.countryExplorer")}
        </MenuItem>
      </Menu>
      <Menu
        menuItemStyles={{
          button: {
            color: "#333",
            backgroundColor: "transparent",
            borderRadius: "8px",
            margin: "4px 8px",
            padding: "10px",
            "&:hover": {
              backgroundColor: "#f0f0f0",
            },
            textTransform: "capitalize",
          },
        }}
      >
        <MenuItem
          icon={<ExitToAppIcon />}
          onClick={logout}
          data-testid="sidebar-logout"
        >
          {t("sidebar.logout")}
        </MenuItem>
      </Menu>
    </Sidebar>
  );
};
