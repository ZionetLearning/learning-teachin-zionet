import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AppSidebar, SidebarLink } from "@ui-components";
import MenuIcon from "@mui/icons-material/Menu";
import TranslateIcon from "@mui/icons-material/Translate";
import ExitToAppIcon from "@mui/icons-material/ExitToApp";
import HomeIcon from "@mui/icons-material/Home";
import PeopleIcon from "@mui/icons-material/People";
import HistoryIcon from "@mui/icons-material/History";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import AssignmentIcon from "@mui/icons-material/Assignment";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import SchoolIcon from "@mui/icons-material/School";
import GBFlag from "country-flag-icons/react/3x2/GB";
import ILFlag from "country-flag-icons/react/3x2/IL";
import { useAuth } from "@app-providers/auth";
import { PreferredLanguageCode } from "@app-providers";
import { useUpdateUserLanguage } from "@app-providers/api";

export const SidebarMenu = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { logout } = useAuth();
  const { t, i18n } = useTranslation();
  const updateLanguage = useUpdateUserLanguage();
  const isHebrew = i18n.language === "he";
  const activePath = location.pathname;
  const handleNavigate = (path: string) => navigate(path);

  const handleLanguageChange = (lang: PreferredLanguageCode) => {
    const previousLang = i18n.language as PreferredLanguageCode;
    i18n.changeLanguage(lang);
    updateLanguage.mutate(lang, {
      onError: () => {
        i18n.changeLanguage(previousLang);
      },
    });
  };

  const toggleItem: SidebarLink = {
    label: t("sidebar.toggleSidebar"),
    icon: <MenuIcon />,
    testId: "toggle-sidebar",
  };

  const items: SidebarLink[] = [
    {
      label: t("sidebar.profile"),
      icon: <AccountCircleIcon />,
      path: "/profile",
      testId: "sidebar-profile",
    },
    {
      label: t("sidebar.home"),
      icon: <HomeIcon />,
      path: "/",
      testId: "sidebar-home",
    },
    {
      label: t("sidebar.users"),
      icon: <PeopleIcon />,
      path: "/users",
      testId: "sidebar-users",
    },
    {
      label: t("sidebar.onlineUsers"),
      icon: <CheckCircleIcon />,
      path: "/online-users",
      testId: "sidebar-online-users",
    },
    {
      label: t("sidebar.classes"),
      icon: <SchoolIcon />,
      path: "/classes",
      testId: "sidebar-classes",
    },
    {
      label: t("sidebar.studentPracticeHistory"),
      icon: <HistoryIcon />,
      path: "/student-practice-history",
      testId: "sidebar-student-practice-history",
    },
    {
      label: t("sidebar.tasks"),
      icon: <AssignmentIcon />,
      path: "/tasks",
      testId: "sidebar-tasks",
    },
  ];

  return (
    <AppSidebar
      items={items}
      toggle={toggleItem}
      dir={isHebrew ? "rtl" : "ltr"}
      activePath={activePath}
      onNavigate={handleNavigate}
      languages={{
        label: t("sidebar.languages"),
        icon: <TranslateIcon />,
        items: [
          {
            code: "he",
            label: t("sidebar.he"),
            icon: <ILFlag style={{ width: 22, height: 16 }} />,
            active: i18n.language === "he",
            onClick: () => handleLanguageChange("he"),
            testId: "sidebar-lang-he",
          },
          {
            code: "en",
            label: t("sidebar.en"),
            icon: <GBFlag style={{ width: 22, height: 16 }} />,
            active: i18n.language === "en",
            onClick: () => handleLanguageChange("en"),
            testId: "sidebar-lang-en",
          },
        ],
      }}
      logoutItem={{
        label: t("sidebar.logout"),
        icon: <ExitToAppIcon />,
        onLogout: logout,
        testId: "sidebar-logout",
      }}
    />
  );
};
