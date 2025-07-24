import { AvatarDa } from "../features";
import { useStyles } from "./style";

export const AvatarDaPage = () => {
  const classes = useStyles();
  return (
    <div className={classes.fullScreenAvatarDaPage}>
      <AvatarDa />
    </div>
  );
};
