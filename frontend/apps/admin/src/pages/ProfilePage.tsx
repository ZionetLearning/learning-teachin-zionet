import { useAuth } from "@app-providers";
import { Profile } from "@ui-components";
export const ProfilePage = () => {
  const { user } = useAuth();
  if (!user) return null;
  return <Profile />;
};
