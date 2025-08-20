import { AvatarDa } from "../features";
import { useStyles } from "./style";

export const AvatarDaPage = () => {
  const classes = useStyles();
  return (
    <div
      className={classes.fullScreenAvatarDaPage}
      data-testid="avatar-da-page"
    >
      <AvatarDa />
    </div>
  );
};
