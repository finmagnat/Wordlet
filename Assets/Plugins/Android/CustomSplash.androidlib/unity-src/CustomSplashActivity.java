package com.unity3d.player;

import android.graphics.Color;
import android.os.Bundle;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageView;

import java.lang.ref.WeakReference;

public class CustomSplashActivity extends UnityPlayerGameActivity {
    private static final int SPLASH_BG_COLOR = Color.rgb(26, 11, 46);
    private static final int LOGO_SIZE_DP = 288;

    private static WeakReference<CustomSplashActivity> currentActivity = new WeakReference<>(null);
    private static boolean hideRequested;

    private View splashOverlay;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        hideRequested = false;
        currentActivity = new WeakReference<>(this);
        super.onCreate(savedInstanceState);
        applySplashSystemBarColors();
    }

    @Override
    protected void onCreateSurfaceView() {
        super.onCreateSurfaceView();
        addSplashOverlay();
    }

    public static void hideSplashScreen() {
        hideRequested = true;

        CustomSplashActivity activity = currentActivity.get();
        if (activity == null) {
            return;
        }

        activity.runOnUiThread(activity::removeSplashOverlay);
    }

    private void addSplashOverlay() {
        if (hideRequested || splashOverlay != null) {
            return;
        }

        FrameLayout overlay = new FrameLayout(this);
        overlay.setBackgroundColor(resolveColor("splash_bg_color", SPLASH_BG_COLOR));
        overlay.setClickable(true);
        overlay.setFocusable(true);

        ImageView logo = new ImageView(this);
        int logoId = getResources().getIdentifier("splash_logo", "drawable", getPackageName());
        if (logoId != 0) {
            logo.setImageResource(logoId);
        }
        logo.setScaleType(ImageView.ScaleType.FIT_CENTER);

        int logoSize = dp(LOGO_SIZE_DP);
        FrameLayout.LayoutParams logoParams = new FrameLayout.LayoutParams(
                logoSize,
                logoSize,
                Gravity.CENTER);
        overlay.addView(logo, logoParams);

        ViewGroup.LayoutParams overlayParams = new ViewGroup.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT);
        addContentView(overlay, overlayParams);

        splashOverlay = overlay;
    }

    private void removeSplashOverlay() {
        if (splashOverlay == null) {
            return;
        }

        ViewGroup parent = (ViewGroup) splashOverlay.getParent();
        if (parent != null) {
            parent.removeView(splashOverlay);
        }

        splashOverlay = null;
    }

    private void applySplashSystemBarColors() {
        int bgColor = resolveColor("splash_bg_color", SPLASH_BG_COLOR);
        getWindow().setStatusBarColor(bgColor);
        getWindow().setNavigationBarColor(bgColor);
    }

    private int resolveColor(String name, int fallbackColor) {
        int colorId = getResources().getIdentifier(name, "color", getPackageName());
        if (colorId == 0) {
            return fallbackColor;
        }

        return getResources().getColor(colorId, getTheme());
    }

    private int dp(int value) {
        return Math.round(value * getResources().getDisplayMetrics().density);
    }

    @Override
    protected void onDestroy() {
        if (currentActivity.get() == this) {
            currentActivity = new WeakReference<>(null);
        }

        removeSplashOverlay();
        super.onDestroy();
    }
}
