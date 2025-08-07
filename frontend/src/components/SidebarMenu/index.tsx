import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
	Sidebar,
	Menu,
	MenuItem,
	SubMenu,
	sidebarClasses,
} from 'react-pro-sidebar';

import MenuIcon from '@mui/icons-material/Menu';
import ChatIcon from '@mui/icons-material/Chat';
import FaceIcon from '@mui/icons-material/Face';
import KeyboardIcon from '@mui/icons-material/Keyboard';
import HomeIcon from '@mui/icons-material/Home';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import PublicIcon from '@mui/icons-material/Public';
import WeatherWidgetIcon from '@mui/icons-material/Cloud';
import LiveTvIcon from '@mui/icons-material/LiveTv';

import { useAuth } from '@/providers/auth';

export const SidebarMenu = () => {
	const navigate = useNavigate();
	const location = useLocation();
	const { logout } = useAuth();
	const [collapsed, setCollapsed] = useState(false);

	const handleNavigation = (path: string) => {
		navigate(path);
	};

	const isActive = (path: string) => location.pathname === path;

	return (
		<Sidebar
			collapsed={collapsed}
			rootStyles={{
				[`.${sidebarClasses.container}`]: {
					backgroundColor: '#f4f4f4',
					borderRight: '1px solid #ddd',
					height: '100vh',
					display: 'flex',
					flexDirection: 'column',
					justifyContent: 'space-between',
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
						'& .ps-menu-icon': {
							color: active ? '#fff' : '#7c4dff',
						},
						'&:hover': {
							backgroundColor: active ? '#6a40e6' : '#f0f0f0',
							color: active ? '#fff' : '#000',
						},
						textTransform: 'capitalize',
					}),
					label: {
						textAlign: 'left',
					},
				}}
			>
				<MenuItem
					icon={<MenuIcon />}
					onClick={() => setCollapsed((prev) => !prev)}
				>
					{!collapsed && 'Toggle Sidebar'}
				</MenuItem>

				<MenuItem
					icon={<HomeIcon />}
					onClick={() => handleNavigation('/')}
					active={isActive('/')}
				>
					Home
				</MenuItem>

				<SubMenu label="Chat Tools" icon={<ChatIcon />}>
					<MenuItem
						onClick={() => handleNavigation('/chat/sh')}
						active={isActive('/chat/sh')}
					>
						Chat - Sh (Shirley)
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/chat/yo')}
						active={isActive('/chat/yo')}
					>
						Chat - Yo (Yonatan)
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/chat/da')}
						active={isActive('/chat/da')}
					>
						Chat - Da (Daniel)
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/chat/ou')}
						active={isActive('/chat/ou')}
					>
						Chat - Ou (Ouriel)
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/chat-avatar')}
						active={isActive('/chat-avatar')}
					>
						Chat - Avatar
					</MenuItem>
				</SubMenu>

				<SubMenu label="Avatar Tools" icon={<FaceIcon />}>
					<MenuItem
						onClick={() => handleNavigation('/avatar/ou')}
						active={isActive('/avatar/ou')}
					>
						Avatar - Ou
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/avatar/sh')}
						active={isActive('/avatar/sh')}
					>
						Avatar - Sh
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/avatar/da')}
						active={isActive('/avatar/da')}
					>
						Avatar - Da
					</MenuItem>
				</SubMenu>

				<SubMenu label="Practice Tools" icon={<KeyboardIcon />}>
					<MenuItem
						onClick={() => handleNavigation('/typing')}
						active={isActive('/typing')}
					>
						Typing Practice
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/word-order-game')}
						active={isActive('/word-order-game')}
					>
						Word Order Game
					</MenuItem>
					<MenuItem
						onClick={() => handleNavigation('/speaking')}
						active={isActive('/speaking')}
					>
						Speaking Practice
					</MenuItem>
				</SubMenu>

				<MenuItem
					icon={<PublicIcon />}
					onClick={() => handleNavigation('/earthquake-map')}
					active={isActive('/earthquake-map')}
				>
					Earthquake Map
				</MenuItem>
				<MenuItem
					onClick={() => handleNavigation('/weather')}
					icon={<WeatherWidgetIcon />}
					active={isActive('/weather')}
				>
					Weather
				</MenuItem>
				<MenuItem
					onClick={() => handleNavigation('/anime-explorer')}
					icon={<LiveTvIcon />}
					active={isActive('/anime-explorer')}
				>
					Anime Explorer
				</MenuItem>
			</Menu>
			<Menu
				menuItemStyles={{
					button: {
						color: '#333',
						backgroundColor: 'transparent',
						borderRadius: '8px',
						margin: '4px 8px',
						padding: '10px',
						'&:hover': {
							backgroundColor: '#f0f0f0',
						},
						textTransform: 'capitalize',
					},
				}}
			>
				<MenuItem icon={<ExitToAppIcon />} onClick={logout}>
					Logout
				</MenuItem>
			</Menu>
		</Sidebar>
	);
};
