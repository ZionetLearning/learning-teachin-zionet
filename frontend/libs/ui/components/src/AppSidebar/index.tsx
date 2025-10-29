import { useEffect, useState } from "react";
import {
  Sidebar,
  Menu,
  MenuItem,
  SubMenu,
  sidebarClasses,
} from "react-pro-sidebar";
import { useThemeColors } from "@app-providers";
import { useColorScheme } from "@mui/material/styles";
import BrightnessAutoIcon from "@mui/icons-material/BrightnessAuto";
import LightModeIcon from "@mui/icons-material/LightMode";
import DarkModeIcon from "@mui/icons-material/DarkMode";
import PaletteIcon from "@mui/icons-material/Palette";

export interface SidebarLink {
  label: React.ReactNode;
  icon?: React.ReactNode;
  path?: string;
  testId?: string;
  children?: SidebarLink[];
  onClick?: () => void;
}

export interface LanguageItem {
  code: string; // 'en' | 'he'
  label: React.ReactNode; // visible label
  icon?: React.ReactNode; // optional flag/icon
  active?: boolean;
  onClick: () => void; // switch language
  testId?: string;
}

export interface AppSidebarProps {
  items: SidebarLink[];
  languages?: {
    label: React.ReactNode;
    icon?: React.ReactNode;
    items: LanguageItem[];
  };
  toggle?: SidebarLink;
  /* notify parent when user toggles */
  onCollapsedChange?: (collapsed: boolean) => void;
  dir?: "ltr" | "rtl";
  activePath?: string;
  onNavigate?: (path: string) => void;
  logoutItem?: {
    label: React.ReactNode;
    icon?: React.ReactNode;
    onLogout: () => void;
    testId?: string;
  };
}

export const AppSidebar = ({
  items,
  languages,
  toggle,
  onCollapsedChange,
  dir = "ltr",
  activePath,
  onNavigate,
  logoutItem,
}: AppSidebarProps) => {
  const [collapsed, setCollapsed] = useState<boolean>(
    typeof window !== "undefined" ? window.innerWidth < 768 : false,
  );

  const { mode, setMode } = useColorScheme(); // 'light' | 'dark' | 'system'
  const color = useThemeColors();

  useEffect(() => {
    const handleResize = () => {
      setCollapsed(window.innerWidth < 768);
    };

    window.addEventListener("resize", handleResize);
    return () => window.removeEventListener("resize", handleResize);
  }, []);

  const handleToggle = () => {
    setCollapsed((prev) => {
      const next = !prev;
      onCollapsedChange?.(next);
      return next;
    });
  };

  const isActive = (path?: string) => Boolean(path) && activePath === path;

  const handleItemClick = (item: SidebarLink) => {
    if (item.onClick) {
      item.onClick();
      return;
    }
    if (item.path && onNavigate) {
      onNavigate(item.path);
    }
  };

  const renderItem = (item: SidebarLink): React.ReactNode => {
    if (item.children?.length) {
      return (
        <SubMenu key={String(item.label)} label={item.label} icon={item.icon}>
          {item.children.map((child) => renderItem(child))}
        </SubMenu>
      );
    }
    return (
      <MenuItem
        key={String(item.label)}
        icon={item.icon}
        active={isActive(item.path)}
        data-testid={item.testId}
        onClick={() => handleItemClick(item)}
      >
        {item.label}
      </MenuItem>
    );
  };

  return (
    <Sidebar
      data-testid="app-sidebar"
      collapsed={collapsed}
      dir={dir}
      rootStyles={{
        [`.${sidebarClasses.container}`]: {
          backgroundColor: color.bg,
          color: color.text,
          height: "100vh",
          display: "flex",
          flexDirection: "column",
          justifyContent: "space-between",
          direction: dir,
          borderRight: `1px solid ${color.divider}`,
        },
      }}
    >
      <Menu
        menuItemStyles={{
          button: ({ active }) => ({
            color: active ? color.primaryContrast : color.text,
            backgroundColor: active ? color.primary : "transparent",
            borderRadius: "8px",
            margin: "4px 8px",
            padding: "10px",
            "& .ps-menu-icon": {
              color: active ? color.primaryContrast : color.primary,
            },
            "&:hover": {
              backgroundColor: active ? color.primaryDark : color.hover,
              color: active ? color.primaryContrast : color.text,
            },
            textTransform: "capitalize",
          }),
          label: { textAlign: dir === "rtl" ? "right" : "left" },
          subMenuContent: {
            backgroundColor: color.bg,
          },
        }}
      >
        {toggle && (
          <MenuItem
            icon={toggle.icon}
            onClick={handleToggle}
            data-testid={toggle.testId}
          >
            {!collapsed && toggle?.label}
          </MenuItem>
        )}

        {languages?.items?.length ? (
          <SubMenu
            label={languages.label}
            icon={languages.icon}
            data-testid="sidebar-languages"
          >
            {languages.items.map((lng) => (
              <MenuItem
                key={lng.code}
                icon={lng.icon}
                onClick={lng.onClick}
                active={Boolean(lng.active)}
                data-testid={lng.testId}
              >
                {lng.label}
              </MenuItem>
            ))}
          </SubMenu>
        ) : null}

        <SubMenu label="Appearance" icon={<PaletteIcon />}>
          <MenuItem
            icon={<BrightnessAutoIcon />}
            active={mode === "system"}
            onClick={() => setMode("system")}
            data-testid="sidebar-theme-system"
          >
            System
          </MenuItem>
          <MenuItem
            icon={<LightModeIcon />}
            active={mode === "light"}
            onClick={() => setMode("light")}
            data-testid="sidebar-theme-light"
          >
            Light
          </MenuItem>
          <MenuItem
            icon={<DarkModeIcon />}
            active={mode === "dark"}
            onClick={() => setMode("dark")}
            data-testid="sidebar-theme-dark"
          >
            Dark
          </MenuItem>
        </SubMenu>

        {items.map(renderItem)}
      </Menu>

      {logoutItem && (
        <Menu
          menuItemStyles={{
            button: {
              color: color.text,
              backgroundColor: "transparent",
              borderRadius: "8px",
              margin: "4px 8px",
              padding: "10px",
              "&:hover": { backgroundColor: color.hover },
              textTransform: "capitalize",
            },
          }}
        >
          <MenuItem
            icon={logoutItem.icon}
            onClick={logoutItem.onLogout}
            data-testid={logoutItem.testId}
          >
            {logoutItem.label}
          </MenuItem>
        </Menu>
      )}
    </Sidebar>
  );
};
