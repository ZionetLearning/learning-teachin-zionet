import { useNavigate, useLocation } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { AppSidebar, SidebarLink } from "@ui-components";
import MenuIcon from "@mui/icons-material/Menu";
import TranslateIcon from "@mui/icons-material/Translate";
import ChatIcon from "@mui/icons-material/Chat";
import FaceIcon from "@mui/icons-material/Face";
import KeyboardIcon from "@mui/icons-material/Keyboard";
import HomeIcon from "@mui/icons-material/Home";
import ExitToAppIcon from "@mui/icons-material/ExitToApp";
import PublicIcon from "@mui/icons-material/Public";
import WeatherWidgetIcon from "@mui/icons-material/Cloud";
import ThreePIcon from "@mui/icons-material/ThreeP";
import LiveTvIcon from "@mui/icons-material/LiveTv";
import AccountCircleIcon from "@mui/icons-material/AccountCircle";
import PendingActionsIcon from "@mui/icons-material/PendingActions";
import DashboardCustomizeIcon from "@mui/icons-material/DashboardCustomize";
import EmojiSymbolsIcon from "@mui/icons-material/EmojiSymbols";
import SchoolIcon from "@mui/icons-material/School";
import CastForEducationIcon from "@mui/icons-material/CastForEducation";
import FlagIcon from "@mui/icons-material/Flag";
import NoteAltIcon from "@mui/icons-material/NoteAlt";
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
    i18n.changeLanguage(lang);
    updateLanguage.mutate(lang);
  };

  const toggleItem: SidebarLink = {
    label: t("sidebar.toggleSidebar"),
    icon: <MenuIcon />,
    testId: "toggle-sidebar",
  };

  const items: SidebarLink[] = [
    {
      label: t("sidebar.home"),
      icon: <HomeIcon />,
      path: "/",
      testId: "sidebar-home",
    },
    {
      label: t("sidebar.profile"),
      icon: <AccountCircleIcon />,
      path: "/profile",
      testId: "sidebar-profile",
    },
    {
      label: t("sidebar.classes"),
      icon: <CastForEducationIcon />,
      path: "/classes",
      testId: "sidebar-classes",
    },
    {
      label: t("sidebar.myLearning"),
      icon: <SchoolIcon />,
      children: [
        {
          label: t("sidebar.wordCards"),
          icon: <DashboardCustomizeIcon />,
          path: "/word-cards",
          testId: "word-cards",
        },
        {
          label: t("sidebar.myPracticeMistakes"),
          icon: <NoteAltIcon />,
          path: "/practice-mistakes",
          testId: "practice-mistakes",
        },
        {
          label: t("sidebar.studentPracticeHistory"),
          icon: <PendingActionsIcon />,
          path: "/practice-history",
          testId: "practice-history",
        },
      ],
    },
    {
      label: t("sidebar.practiceTools"),
      icon: <KeyboardIcon />,
      children: [
        {
          label: t("sidebar.wordOrderGame"),
          path: "/word-order-game",
          testId: "sidebar-word-order",
        },
        {
          label: t("sidebar.typingPractice"),
          path: "/typing",
          testId: "sidebar-typing",
        },
        {
          label: t("sidebar.speakingPractice"),
          path: "/speaking",
          testId: "sidebar-speaking",
        },
        {
          label: t("sidebar.wordCardsChallenge"),
          path: "/word-cards-challenge",
          testId: "word-cards-challenge",
        },
      ],
    },
    {
      label: t("sidebar.chatAvatar"),
      icon: <ThreePIcon />,
      path: "/chat-with-avatar",
      testId: "sidebar-chat-avatar",
    },
    {
      label: t("sidebar.sandbox"),
      icon: <EmojiSymbolsIcon />,
      children: [
        {
          label: t("sidebar.chatTools"),
          icon: <ChatIcon />,
          children: [
            {
              label: t("sidebar.chatYo"),
              path: "/chat/yo",
              testId: "sidebar-chat-yo",
            },
            {
              label: t("sidebar.chatDa"),
              path: "/chat/da",
              testId: "sidebar-chat-da",
            },
            {
              label: t("sidebar.chatOu"),
              path: "/chat/ou",
              testId: "sidebar-chat-ou",
            },
          ],
        },
        {
          label: t("sidebar.avatarTools"),
          icon: <FaceIcon />,
          children: [
            {
              label: t("sidebar.avatarOu"),
              path: "/avatar/ou",
              testId: "sidebar-avatar-ou",
            },
            {
              label: t("sidebar.avatarSh"),
              path: "/avatar/sh",
              testId: "sidebar-avatar-sh",
            },
            {
              label: t("sidebar.avatarDa"),
              path: "/avatar/da",
              testId: "sidebar-avatar-da",
            },
          ],
        },
        {
          label: t("sidebar.earthquakeMap"),
          icon: <PublicIcon />,
          path: "/earthquake-map",
          testId: "sidebar-earthquake",
        },
        {
          label: t("sidebar.weather"),
          icon: <WeatherWidgetIcon />,
          path: "/weather",
          testId: "sidebar-weather",
        },
        {
          label: t("sidebar.anime"),
          icon: <LiveTvIcon />,
          path: "/anime-explorer",
          testId: "sidebar-anime",
        },
        {
          label: t("sidebar.countryExplorer"),
          icon: <FlagIcon />,
          path: "/country-explorer",
        },
      ],
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
