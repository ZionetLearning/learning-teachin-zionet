import { useState } from "react";
import { Box, Tabs, Tab } from "@mui/material";
import SportsEsportsIcon from "@mui/icons-material/SportsEsports";
import EmojiEventsIcon from "@mui/icons-material/EmojiEvents";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import { useTranslation } from "react-i18next";
import { AchievementsSummary } from "../AchievementsSummary";
import { GamesSummary } from "../GamesSummary";
import { WordCardsSummary } from "../WordCardsSummary";
import { useStyles } from "./style";

interface Props {
  userId: string;
  startDate: string;
  endDate: string;
}

type TabValue = "games" | "achievements" | "wordCards";

export const DetailedSummary = ({ userId, startDate, endDate }: Props) => {
  const { t } = useTranslation();
  const classes = useStyles();
  const [activeTab, setActiveTab] = useState<TabValue>("achievements");

  const handleTabChange = (_: React.SyntheticEvent, newValue: TabValue) => {
    setActiveTab(newValue);
  };

  return (
    <Box className={classes.container}>
      <Tabs
        value={activeTab}
        onChange={handleTabChange}
        variant="fullWidth"
        className={classes.tabs}
      >
        <Tab
          value="games"
          icon={<SportsEsportsIcon />}
          label={t("pages.summary.tabs.games")}
          iconPosition="start"
          className={classes.tab}
        />
        <Tab
          value="achievements"
          icon={<EmojiEventsIcon />}
          label={t("pages.summary.tabs.achievements")}
          iconPosition="start"
          className={classes.tab}
        />
        <Tab
          value="wordCards"
          icon={<MenuBookIcon />}
          label={t("pages.summary.tabs.wordCards")}
          iconPosition="start"
          className={classes.tab}
        />
      </Tabs>

      <Box className={classes.content}>
        {activeTab === "games" && (
          <GamesSummary userId={userId} startDate={startDate} endDate={endDate} />
        )}
        {activeTab === "achievements" && (
          <AchievementsSummary userId={userId} startDate={startDate} endDate={endDate} />
        )}
        {activeTab === "wordCards" && (
          <WordCardsSummary userId={userId} startDate={startDate} endDate={endDate} />
        )}
      </Box>
    </Box>
  );
};
