import { useRef, ChangeEvent } from "react";
import { useTranslation } from "react-i18next";
import { Box, Avatar, IconButton, CircularProgress } from "@mui/material";
import PhotoCameraIcon from "@mui/icons-material/PhotoCamera";
import DeleteIcon from "@mui/icons-material/Delete";
import { toast } from "react-toastify";
import {
  useGetAvatarUploadUrl,
  useConfirmAvatar,
  useGetAvatarUrl,
  useDeleteAvatar,
  useUploadToBlob,
} from "@api";
import { useStyles } from "./style";

type AvatarUploadProps = {
  userId: string;
  userName: string;
};

const MAX_FILE_SIZE = 1048576; // 1MB
const ACCEPTED_TYPES = ["image/png", "image/jpeg"];
const ACCEPTED_EXTENSIONS = [".png", ".jpg", ".jpeg"];

export const AvatarUpload = ({ userId, userName }: AvatarUploadProps) => {
  const classes = useStyles();
  const { t } = useTranslation();
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { data: avatarUrl, isLoading: isLoadingUrl } = useGetAvatarUrl(userId);
  const { mutateAsync: getUploadUrl } = useGetAvatarUploadUrl(userId);
  const { mutateAsync: uploadToBlob, isPending: isUploadingToBlob } =
    useUploadToBlob();
  const { mutateAsync: confirmAvatar, isPending: isConfirming } =
    useConfirmAvatar(userId);
  const { mutateAsync: deleteAvatar, isPending: isDeleting } =
    useDeleteAvatar(userId);

  const handleDeleteSuccess = () => {
    toast.success(t("pages.profile.avatar.deleteSuccess"));
  };

  const handleDeleteError = () => {
    toast.error(t("pages.profile.avatar.deleteFailed"));
  };

  const handleUploadSuccess = () => {
    toast.success(t("pages.profile.avatar.uploadSuccess"));
  };

  const handleFileSelect = () => {
    fileInputRef.current?.click();
  };

  const validateFile = (file: File): string | null => {
    if (!ACCEPTED_TYPES.includes(file.type)) {
      const extension = file.name.split(".").pop()?.toLowerCase();
      if (!extension || !ACCEPTED_EXTENSIONS.includes(`.${extension}`)) {
        return t("pages.profile.avatar.invalidType");
      }
    }

    if (file.size > MAX_FILE_SIZE) {
      return t("pages.profile.avatar.fileTooLarge");
    }

    if (!file.type.startsWith("image/")) {
      return t("pages.profile.avatar.invalidType");
    }

    return null;
  };

  const handleFileChange = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }

    if (!file) return;

    const validationError = validateFile(file);
    if (validationError) {
      toast.error(validationError);
      return;
    }

    try {
      const uploadData = await getUploadUrl({
        contentType: file.type,
        sizeBytes: file.size,
      });

      if (!uploadData?.uploadUrl || !uploadData?.blobPath) {
        throw new Error("Invalid upload URL received from server");
      }

      await uploadToBlob({
        uploadUrl: uploadData.uploadUrl,
        file,
      });

      await confirmAvatar({
        blobPath: uploadData.blobPath,
        contentType: file.type,
      });

      handleUploadSuccess();
    } catch (error) {
      console.error("Avatar upload failed:", error);
      toast.error(t("pages.profile.avatar.uploadFailedGeneric"));
    }
  };

  const handleDelete = async () => {
    if (window.confirm(t("pages.profile.avatar.confirmDelete"))) {
      try {
        await deleteAvatar();
        handleDeleteSuccess();
      } catch (error) {
        console.error("Failed to delete avatar:", error);
        handleDeleteError();
      }
    }
  };

  const getInitials = () => {
    if (!userName || userName.length === 0) {
      return "??";
    }

    const names = userName
      .trim()
      .split(" ")
      .filter((name) => name.length > 0);

    if (names.length >= 2 && names[0].length > 0 && names[1].length > 0) {
      return `${names[0][0]}${names[1][0]}`.toUpperCase();
    }

    if (userName.length >= 2) {
      return userName.substring(0, 2).toUpperCase();
    }

    return userName.length === 1 ? userName.toUpperCase() : "?";
  };

  const isLoading =
    isLoadingUrl || isUploadingToBlob || isConfirming || isDeleting;

  return (
    <Box className={classes.container}>
      <Box className={classes.avatarWrapper}>
        <Avatar
          src={avatarUrl || undefined}
          alt={userName}
          className={classes.avatar}
        >
          {!avatarUrl && getInitials()}
        </Avatar>
        {isLoading && (
          <Box className={classes.loadingOverlay}>
            <CircularProgress size={40} />
          </Box>
        )}
      </Box>

      <Box className={classes.buttonGroup}>
        <IconButton
          onClick={handleFileSelect}
          disabled={isLoading}
          className={classes.iconButton}
          title={t("pages.profile.avatar.upload")}
        >
          <PhotoCameraIcon />
        </IconButton>

        {avatarUrl && (
          <IconButton
            onClick={handleDelete}
            disabled={isLoading}
            className={classes.iconButton}
            title={t("pages.profile.avatar.delete")}
          >
            <DeleteIcon />
          </IconButton>
        )}
      </Box>

      <input
        ref={fileInputRef}
        type="file"
        accept={ACCEPTED_TYPES.join(",")}
        onChange={handleFileChange}
        style={{ display: "none" }}
      />
    </Box>
  );
};
