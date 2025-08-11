import avatar from "@/assets/avatar.svg";
import { useStyles } from "./style";

export const AvatarView = ({
  currentVisemeSrc,
}: {
  currentVisemeSrc: string;
}) => {
  const classes = useStyles();
  return (
    <div className={classes.wrapper}>
      <img src={avatar} alt="Avatar" className={classes.avatar} />
      <img src={currentVisemeSrc} alt="Lips" className={classes.lipsImage} />
    </div>
  );
};
