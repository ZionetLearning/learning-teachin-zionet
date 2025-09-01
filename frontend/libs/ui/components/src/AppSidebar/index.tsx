import { useState } from 'react';
import { Sidebar, Menu, MenuItem, SubMenu, sidebarClasses } from 'react-pro-sidebar';

export interface SidebarLink {
  label: React.ReactNode;
  icon?: React.ReactNode;
  path?: string;
  onClick?: () => void;
  testId?: string;
  children?: SidebarLink[];
}

export interface LanguageItem {
  code: string;           // 'en' | 'he'
  label: React.ReactNode; // visible label
  icon?: React.ReactNode; // optional flag/icon
  active?: boolean;
  onClick: () => void;    // switch language
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
  /* initial state only (the component manages the rest) */
  defaultCollapsed?: boolean;
  /* notify parent when user toggles */
  onCollapsedChange?: (collapsed: boolean) => void;
  dir?: 'ltr' | 'rtl';
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
  defaultCollapsed = false,
  onCollapsedChange,
  dir = 'ltr',
  activePath,
  onNavigate,
  logoutItem,
}: AppSidebarProps) => {
  const [collapsed, setCollapsed] = useState<boolean>(defaultCollapsed);

  const handleToggle = () => {
    setCollapsed(prev => {
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
          backgroundColor: '#f4f4f4',
          borderRight: '1px solid #ddd',
          height: '100vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'space-between',
          direction: dir,
        },
      }}
    >
      <Menu
        menuItemStyles={{
          button: ({ active }) => ({
            color: active ? 'white' : '#333',
            backgroundColor: active ? '#7c4dff' : 'transparent',
            borderRadius: '8px',
            margin: '4px 8px',
            padding: '10px',
            '& .ps-menu-icon': { color: active ? '#fff' : '#7c4dff' },
            '&:hover': { backgroundColor: active ? '#6a40e6' : '#f0f0f0', color: active ? '#fff' : '#000' },
            textTransform: 'capitalize',
          }),
          label: { textAlign: dir === 'rtl' ? 'right' : 'left' },
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
          <SubMenu label={languages.label} icon={languages.icon} data-testid="sidebar-languages">
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

        {items.map(renderItem)}
      </Menu>

      {logoutItem && (
        <Menu
          menuItemStyles={{
            button: {
              color: '#333',
              backgroundColor: 'transparent',
              borderRadius: '8px',
              margin: '4px 8px',
              padding: '10px',
              '&:hover': { backgroundColor: '#f0f0f0' },
              textTransform: 'capitalize',
            },
          }}
        >
          <MenuItem icon={logoutItem.icon} onClick={logoutItem.onLogout} data-testid={logoutItem.testId}>
            {logoutItem.label}
          </MenuItem>
        </Menu>
      )}
    </Sidebar>
  );
};
