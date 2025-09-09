import { useAuth } from "@app-providers";
import { Profile } from "@ui-components";
export const ProfilePage = () => {
  const { user } = useAuth();
  if (!user) return null;
  return (
    <Profile firstName={user?.firstName ?? ""}
      lastName={user?.lastName ?? ""}
      email={user?.email ?? ""}
      userId={user?.userId ?? ""} />
  );
}