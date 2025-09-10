import { useAuth } from "@app-providers";
import { Profile } from "@ui-components";

export const ProfilePage = () => {
  const { user } = useAuth();

  if (!user) {
    return <div>Loading...</div>; // or a spinner, or redirect to login
  }
  return <Profile user={user} />;
};
