import { Box } from "@mui/material";
import { useAuth } from "@app-providers";
import { Profile } from "@ui-components";
import { useGetUserAchievements } from "../api/achievements";
import { AchievementsSection } from "../components";

export const ProfilePage = () => {
  const { user } = useAuth();
  const { data: achievements = [], isLoading } = useGetUserAchievements(
    user?.userId,
  );

  if (!user) {
    return <div>Loading...</div>;
  }

  return (
    <Box>
      <Profile user={user} />
      {!isLoading && achievements.length > 0 && (
        <Box sx={{ padding: 3, paddingTop: 0 }}>
          <AchievementsSection achievements={achievements} />
        </Box>
      )}
    </Box>
  );
};
