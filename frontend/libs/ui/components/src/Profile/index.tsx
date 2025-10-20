import { useState, useEffect } from "react";
import { useTranslation } from "react-i18next";
import { Typography, TextField, Stack, Box, Grid } from "@mui/material";
import { useUpdateUserByUserId, toAppRole } from "@app-providers";
import {
  User,
  HebrewLevelValue,
  PreferredLanguageCode,
} from "@app-providers/types";
import { useStyles } from "./style";
import { Dropdown, Button } from "@ui-components";

export const Profile = ({ user }: { user: User }) => {
  const classes = useStyles();

  const { t, i18n } = useTranslation();
  const isRTL = i18n.dir() === "rtl";

  const { mutateAsync: updateUserMutation } = useUpdateUserByUserId(
    user?.userId ?? "",
  );

  const [userDetails, setUserDetails] = useState({
    firstName: user?.firstName ?? "",
    lastName: user?.lastName ?? "",
    hebrewLevelValue: user?.hebrewLevelValue ?? "beginner",
    preferredLanguageCode: user?.preferredLanguageCode ?? "en",
  });

  useEffect(() => {
    setUserDetails({
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      hebrewLevelValue: user?.hebrewLevelValue ?? "beginner",
      preferredLanguageCode: user?.preferredLanguageCode ?? "en",
    });
  }, [
    user?.firstName,
    user?.hebrewLevelValue,
    user?.lastName,
    user?.preferredLanguageCode,
    user.userId,
  ]);

  const handleTextChange =
    (field: "firstName" | "lastName") =>
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setUserDetails((prev) => ({
        ...prev,
        [field]: e.target.value,
      }));
    };

  const handleDropdownChange = (field: "hebrewLevelValue") => (val: string) => {
    setUserDetails((prev) => ({
      ...prev,
      [field]: val as HebrewLevelValue,
    }));
  };

  const handleLanguageChange = (val: string) => {
    setUserDetails((prev) => ({
      ...prev,
      preferredLanguageCode: val as PreferredLanguageCode,
    }));
  };

  const handleCancel = () => {
    setUserDetails({
      firstName: user?.firstName ?? "",
      lastName: user?.lastName ?? "",
      hebrewLevelValue: user?.hebrewLevelValue ?? "beginner",
      preferredLanguageCode: user?.preferredLanguageCode ?? "en",
    });
  };

  const handleSave = async () => {
    await updateUserMutation({
      email: user.email,
      firstName: userDetails.firstName.trim(),
      lastName: userDetails.lastName.trim(),
      hebrewLevelValue: userDetails.hebrewLevelValue,
      preferredLanguageCode: userDetails.preferredLanguageCode,
    });
  };

  const dirty =
    userDetails.firstName.trim() !== (user?.firstName ?? "").trim() ||
    userDetails.lastName.trim() !== (user?.lastName ?? "").trim() ||
    userDetails.hebrewLevelValue !== (user?.hebrewLevelValue ?? "beginner") ||
    userDetails.preferredLanguageCode !== (user?.preferredLanguageCode ?? "en");

  const hebrewLevelOptions = [
    { value: "beginner", label: t("hebrewLevels.beginner") },
    { value: "intermediate", label: t("hebrewLevels.intermediate") },
    { value: "advanced", label: t("hebrewLevels.advanced") },
    { value: "fluent", label: t("hebrewLevels.fluent") },
  ];

  const languageOptions = [
    { value: "he", label: t("languages.hebrew") },
    { value: "en", label: t("languages.english") },
  ];

  return (
    <Box className={classes.container}>
      <Box className={classes.titleContainer}>
        <Typography variant="h4" className={classes.title}>
          {t("pages.profile.title")}
        </Typography>
      </Box>

      <Box className={classes.formCard}>
        <Box className={classes.formHeader}>
          <Typography variant="h6" color="text.secondary">
            {t("pages.profile.subTitle")}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {t("pages.profile.secondSubTitle")}
          </Typography>
        </Box>

        <Stack spacing={3}>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box className={classes.fieldContainer}>
                <Typography
                  variant="body2"
                  color="text.primary"
                  className={
                    isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR
                  }
                >
                  {t("pages.profile.firstName")}
                </Typography>
                <TextField
                  value={userDetails.firstName}
                  onChange={handleTextChange("firstName")}
                  fullWidth
                  className={
                    isRTL ? classes.textFieldRTL : classes.textFieldLTR
                  }
                  size="small"
                />
              </Box>
            </Grid>
            <Grid size={{ xs: 12, md: 6 }}>
              <Box className={classes.fieldContainer}>
                <Typography
                  variant="body2"
                  color="text.primary"
                  className={
                    isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR
                  }
                >
                  {t("pages.profile.lastName")}
                </Typography>
                <TextField
                  value={userDetails.lastName}
                  onChange={handleTextChange("lastName")}
                  fullWidth
                  className={
                    isRTL ? classes.textFieldRTL : classes.textFieldLTR
                  }
                  size="small"
                />
              </Box>
            </Grid>
          </Grid>

          <Box className={classes.fieldContainer}>
            {toAppRole(user?.role) === "student" && (
              <Box className={classes.fieldContainer}>
                <Typography
                  variant="body2"
                  color="text.primary"
                  className={
                    isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR
                  }
                >
                  {t("hebrewLevels.title")}
                </Typography>
                <Dropdown
                  name="hebrewLevel"
                  options={hebrewLevelOptions}
                  value={userDetails.hebrewLevelValue}
                  onChange={(val) =>
                    handleDropdownChange("hebrewLevelValue")(val)
                  }
                />
              </Box>
            )}

            <Box className={classes.fieldContainer}>
              <Typography
                variant="body2"
                color="text.primary"
                className={
                  isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR
                }
              >
                {t("pages.profile.preferredLanguage")}
              </Typography>
              <Box className={classes.dropdown}>
                <Dropdown
                  name="preferredLanguage"
                  options={languageOptions}
                  value={userDetails.preferredLanguageCode}
                  onChange={handleLanguageChange}
                />
              </Box>
            </Box>

            <Typography
              variant="body2"
              color="text.primary"
              className={isRTL ? classes.fieldLabelRTL : classes.fieldLabelLTR}
            >
              {t("pages.profile.email")}
            </Typography>
            <TextField
              value={user?.email}
              disabled
              fullWidth
              className={isRTL ? classes.textFieldRTL : classes.textFieldLTR}
              size="small"
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
          </Box>
        </Stack>

        <Box className={classes.buttonContainer}>
          <Button onClick={handleSave} disabled={!dirty}>
            {t("pages.profile.saveChanges")}
          </Button>
          <Button variant="outlined" disabled={!dirty} onClick={handleCancel}>
            {t("pages.profile.cancel")}
          </Button>
        </Box>
      </Box>
    </Box>
  );
};
