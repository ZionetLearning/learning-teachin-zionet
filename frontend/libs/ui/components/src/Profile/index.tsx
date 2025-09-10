import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Typography, TextField, Stack } from "@mui/material";
import { useUpdateUserByUserId, useAuth } from "@app-providers";
import { Button } from "../Button";
import { useStyles } from "./style";

export const Profile = () => {
  const { t, i18n } = useTranslation();
  const classes = useStyles();

  const { user } = useAuth();
  const { mutateAsync: updateUserMutation } = useUpdateUserByUserId(
    user?.userId ?? "",
  );

  const [fn, setFn] = useState<string>("");
  const [ln, setLn] = useState<string>("");
  const isRTL = i18n.dir() === "rtl";

  useEffect(() => {
    if (user?.firstName !== undefined) setFn(user.firstName);
    if (user?.lastName !== undefined) setLn(user.lastName);
  }, [user?.firstName, user?.lastName, user?.userId]);

  if (!user) return null;

  const dirty =
    fn.trim() !== (user?.firstName ?? "").trim() ||
    ln.trim() !== (user?.lastName ?? "").trim();

  const handleCancel = () => {
    setFn(user?.firstName ?? "");
    setLn(user?.lastName ?? "");
  };

  const handleSave = async () => {
    await updateUserMutation({
      email: user.email,
      firstName: fn.trim(),
      lastName: ln.trim(),
    });
  };

  return (
    <div className={classes.container}>
      <div className={classes.titleContainer}>
        <Typography variant="h4" className={classes.title}>
          {t("pages.profile.title")}
        </Typography>
      </div>

      <div className={classes.formCard}>
        <div className={classes.formHeader}>
          <Typography variant="h6">{t("pages.profile.subTitle")}</Typography>
          <Typography variant="body2" color="text.secondary">
            {t("pages.profile.secondSubTitle")}
          </Typography>
        </div>

        <Stack spacing={3}>
          <div className={classes.fieldContainer}>
            <Typography
              variant="body2"
              color="text.primary"
              className={isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR}
            >
              {t("pages.profile.firstName")}
            </Typography>
            <TextField
              value={fn}
              onChange={(e) => {
                setFn(e.target.value);
              }}
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
            />
          </div>

          <div className={classes.fieldContainer}>
            <Typography
              variant="body2"
              color="text.primary"
              className={isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR}
            >
              {t("pages.profile.lastName")}
            </Typography>
            <TextField
              value={ln}
              onChange={(e) => {
                setLn(e.target.value);
              }}
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
            />
          </div>

          <div className={classes.fieldContainer}>
            <Typography
              variant="body2"
              color="text.primary"
              className={
                isRTL ? classes.emailFieldLabelRTL : classes.emailFieldLabelLTR
              }
            >
              {t("pages.profile.email")}
            </Typography>
            <TextField
              value={user?.email}
              disabled
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
            />
            <Typography
              variant="body2"
              color="text.disabled"
              className={
                isRTL
                  ? classes.emailDisabledNoteRTL
                  : classes.emailDisabledNoteLTR
              }
            >
              {t("pages.profile.emailCannotBeChanged")}
            </Typography>
          </div>
        </Stack>

        <div className={classes.buttonContainer}>
          <Button onClick={handleSave} disabled={!dirty}>
            {t("pages.profile.saveChanges")}
          </Button>
          <Button variant="outlined" disabled={!dirty} onClick={handleCancel}>
            {t("pages.profile.cancel")}
          </Button>
        </div>
      </div>
    </div>
  );
};
