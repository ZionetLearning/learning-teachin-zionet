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
import GBFlag from "country-flag-icons/react/3x2/GB";
import ILFlag from "country-flag-icons/react/3x2/IL";
import { useAuth } from "@/providers/auth";

export const SidebarMenu = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout } = useAuth();
  const { t, i18n } = useTranslation();
  const [collapsed, setCollapsed] = useState(false);
  const [langActive, setLangActive] = useState<"he" | "en" | null>("en");
  const flagSize = { width: 22, height: 16 };
  const handleNavigation = (path: string) => {
    navigate(path);
  };

  const isHebrew = i18n.language === "he";
  const isActive = (path: string) => location.pathname === path;

  const changeLang = (lng: "en" | "he") => () => {
    i18n.changeLanguage(lng);
    setLangActive(lng);
  };
  return (
    <Sidebar
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

        <SubMenu label={t("sidebar.languages")} icon={<TranslateIcon />}>
          <MenuItem
            icon={<ILFlag style={flagSize} />}
            onClick={changeLang("he")}
            active={langActive === "he"}
          >
            {t("sidebar.he")}
          </MenuItem>

          <MenuItem
            icon={<GBFlag style={flagSize} />}
            onClick={changeLang("en")}
            active={langActive === "en"}
          >
            {t("sidebar.en")}
          </MenuItem>
        </SubMenu>

        <MenuItem
          icon={<HomeIcon />}
          onClick={() => handleNavigation("/")}
          active={isActive("/")}
        >
          {t("sidebar.home")}
        </MenuItem>

         <MenuItem
          icon={<HomeIcon />}
          onClick={() => handleNavigation("/chat-with-avatar")}
          active={isActive("/")}
        >
          {t("sidebar.chatWithAvatar")}
        </MenuItem>

        <SubMenu label={t("sidebar.chatTools")} icon={<ChatIcon />}>
          <MenuItem
            onClick={() => handleNavigation("/chat/sh")}
            active={isActive("/chat/sh")}
          >
            {t("sidebar.chatSh")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/chat/yo")}
            active={isActive("/chat/yo")}
          >
            {t("sidebar.chatYo")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/chat/da")}
            active={isActive("/chat/da")}
          >
            {t("sidebar.chatDa")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/chat/ou")}
            active={isActive("/chat/ou")}
          >
            {t("sidebar.chatOu")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/chat-avatar")}
            active={isActive("/chat-avatar")}
          >
            {t("sidebar.chatAvatar")}
          </MenuItem>
        </SubMenu>

        <SubMenu label={t("sidebar.avatarTools")} icon={<FaceIcon />}>
          <MenuItem
            onClick={() => handleNavigation("/avatar/ou")}
            active={isActive("/avatar/ou")}
          >
            {t("sidebar.avatarOu")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/avatar/sh")}
            active={isActive("/avatar/sh")}
          >
            {t("sidebar.avatarSh")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/avatar/da")}
            active={isActive("/avatar/da")}
          >
            {t("sidebar.avatarDa")}
          </MenuItem>
        </SubMenu>

        <SubMenu label={t("sidebar.practiceTools")} icon={<KeyboardIcon />}>
          <MenuItem
            onClick={() => handleNavigation("/typing")}
            active={isActive("/typing")}
          >
            {t("sidebar.typingPractice")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/word-order-game")}
            active={isActive("/word-order-game")}
          >
            {t("sidebar.wordOrderGame")}
          </MenuItem>
          <MenuItem
            onClick={() => handleNavigation("/speaking")}
            active={isActive("/speaking")}
          >
            {t("sidebar.speakingPractice")}
          </MenuItem>
        </SubMenu>

        <MenuItem
          icon={<PublicIcon />}
          onClick={() => handleNavigation("/earthquake-map")}
          active={isActive("/earthquake-map")}
        >
          {t("sidebar.earthquakeMap")}
        </MenuItem>
        <MenuItem
          onClick={() => handleNavigation("/weather")}
          icon={<WeatherWidgetIcon />}
          active={isActive("/weather")}
        >
          {t("sidebar.weather")}
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
        <MenuItem icon={<ExitToAppIcon />} onClick={logout}>
          {t("sidebar.logout")}
        </MenuItem>
      </Menu>
    </Sidebar>
  );
};
