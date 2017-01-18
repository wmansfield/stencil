<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">

  
<xsl:for-each select="items/item">
  <xsl:variable name="name_lowered"><xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@name"/></xsl:call-template></xsl:variable>
  <xsl:variable name="removePattern2"><xsl:value-of select="@removePattern"/></xsl:variable>
'''[STARTFILE:..\..\Generation\Output\UIHelpers\Aurelia\<xsl:value-of select="$name_lowered"/>s.html]
&lt;template&gt;
  <xsl:if test="not(@uiAsChild='true')">&lt;require from="admins/admin-bar"&gt;&lt;/require&gt;</xsl:if>
  &lt;require from="../shared/pager"&gt;&lt;/require&gt;
  &lt;require from="../shared/search-bar"&gt;&lt;/require&gt;

  &lt;section class="au-animate"&gt;
    <xsl:if test="not(@uiAsChild='true')">&lt;div class="container"&gt;</xsl:if>
      <xsl:if test="not(@uiAsChild='true')">&lt;admin-bar router.bind="router" app.bind="app" area="admin" route="<xsl:value-of select="$name_lowered"/>s" &gt;&lt;/admin-bar&gt;</xsl:if>
      
        &lt;h3&gt;&lt;i class="fa fa-codepen icon-spaced"&gt;&lt;/i&gt;${'<xsl:value-of select="@friendlyName"/>s' |t}
          &lt;a role="button" click.delegate="refresh()" class="icon-small icon-spaced"&gt;&lt;i class="fa fa-refresh" if.bind="!refreshing"&gt;&lt;/i&gt;&lt;i class="fa fa-refresh fa-spin" if.bind="refreshing"&gt;&lt;/i&gt; &lt;/a&gt;
          &lt;a data-toggle="modal" href="#createModal<xsl:value-of select="@name"/>" class="btn btn-secondary demo-element pull-right"&gt;${'Create New <xsl:value-of select="@friendlyName"/>' |t}&lt;/a&gt;
          &lt;search-bar app.bind="app" service.bind="pageService" entity="<xsl:value-of select="@friendlyName"/>"&gt;&lt;/search-bar&gt;
        &lt;/h3&gt;
        &lt;table class="table table-hover table-bordered"&gt;
            &lt;thead&gt;
            &lt;tr&gt;
                <xsl:for-each select="field[@type!='Guid' and not(@uiListHidden='true')]">&lt;td&gt;${'<xsl:value-of select="@friendlyName"/>' |t}&lt;/td&gt;
                </xsl:for-each>
                &lt;td&gt;#&lt;/td&gt;
            &lt;/tr&gt;
            &lt;/thead&gt;
            &lt;tbody&gt;
            &lt;tr repeat.for="item of data.items"&gt;
                <xsl:for-each select="field[@type!='Guid' and not(@uiListHidden='true') and string-length(@priorityGroupBy)=0]">&lt;td&gt;${item.<xsl:value-of select="text()"/>}&lt;/td&gt;
                </xsl:for-each>
                <xsl:for-each select="field[not(@uiListHidden='true') and string-length(@priorityGroupBy)>0]">&lt;td align="center" &gt;
                    &lt;div class="btn-group"&gt;
                        &lt;button type="button" class="btn btn-default dropdown-toggle" data-toggle="dropdown" style="min-width:42px;" &gt;&lt;i class="fa fa-refresh fa-spin" if.bind="item.submitting"&gt;&lt;/i&gt; ${item.<xsl:value-of select="text()"/>}&lt;/button&gt;
                        &lt;ul class="dropdown-menu" role="menu"&gt;
                            &lt;li&gt;&lt;a role="button" click.delegate="doUpdate<xsl:value-of select="@friendlyName"/>(item,-1,true)"&gt;&lt;i class="fa fa-angle-double-up dropdown-icon "&gt;&lt;/i&gt;${'Move To Top' |t}&lt;/a&gt;&lt;/li&gt;
                            &lt;li&gt;&lt;a role="button" click.delegate="doUpdate<xsl:value-of select="@friendlyName"/>(item,-1,false)"&gt;&lt;i class="fa fa-angle-up dropdown-icon "&gt;&lt;/i&gt;${'Move Up' |t}&lt;/a&gt;&lt;/li&gt;
                            &lt;li&gt;&lt;a role="button" click.delegate="doUpdate<xsl:value-of select="@friendlyName"/>(item,1,false)"&gt;&lt;i class="fa fa-angle-down dropdown-icon "&gt;&lt;/i&gt;${'Move Down' |t}&lt;/a&gt;&lt;/li&gt;
                            &lt;li&gt;&lt;a role="button" click.delegate="doUpdate<xsl:value-of select="@friendlyName"/>(item,1,true)"&gt;&lt;i class="fa fa-angle-double-down dropdown-icon "&gt;&lt;/i&gt;${'Move To Bottom' |t}&lt;/a&gt;&lt;/li&gt;
                        &lt;/ul&gt;
                    &lt;/div&gt;
                &lt;/td&gt;
                </xsl:for-each>
                &lt;td&gt;&lt;a class="btn-default btn btn-sm" data-toggle="modal" href="#editModal<xsl:value-of select="@name"/>" click.delegate="showEdit(item.<xsl:value-of select="field[1]"/>)" role="button"&gt;${'Edit' |t}&lt;/a&gt;&lt;/td&gt;
            &lt;/tr&gt;
            &lt;/tbody&gt;
        &lt;/table&gt;
        &lt;div class="hvn-table-footer"&gt;
            &lt;pager data.bind="data" app.bind="app" step.bind="sort.step" service.bind="pageService"&gt;&lt;/pager&gt;
         &lt;/div&gt;
    <xsl:if test="not(@uiAsChild='true')">&lt;/div&gt;</xsl:if>
    &lt;br/&gt;
    &lt;br/&gt;
    
    &lt;div id="createModal<xsl:value-of select="@name"/>" class="modal modal-styled fade" tabindex="-1" role="dialog"&gt;
        &lt;div class="modal-dialog modal-lg"&gt;
          &lt;div class="modal-content"&gt;

            &lt;div class="modal-header"&gt;
              &lt;button type="button" class="close" data-dismiss="modal" aria-hidden="true"&gt;×&lt;/button&gt;
              &lt;h3 class="modal-title" &gt;&lt;i class="fa fa-codepen icon-spaced"&gt;&lt;/i&gt;${'Create <xsl:value-of select="@friendlyName"/>' |t}&lt;/h3&gt;
            &lt;/div&gt; &lt;!-- /.modal-header --&gt;

            &lt;div class="modal-body"&gt;
              
                  &lt;form id="createForm<xsl:value-of select="@name"/>" role="form" class="form parsley-form" submit.delegate="doCreate()" &gt;
                    <xsl:for-each select="field[not(@uiCreateHidden='true')]">
                    <xsl:variable name="foreignKey"><xsl:value-of select="@foreignKey"/></xsl:variable>
                    <xsl:choose><xsl:when test="string-length(@foreignKey)>0">&lt;div class="form-group"&gt;
                      &lt;label class="control-label" for="<xsl:value-of select="text()"/>"&gt;${'<xsl:value-of select="@friendlyName"/>' |t}&lt;/label&gt;
                      &lt;select id="<xsl:value-of select="text()"/>" name="<xsl:value-of select="text()"/>" value.bind="create.<xsl:value-of select="text()"/>" class="form-control" data-parsley-required="<xsl:value-of select="@isNullable='false'"/>" &gt;
                          &lt;option value=""&gt;${'Please Select <xsl:value-of select="@friendlyName"/>' |t}&lt;/option&gt;
                          &lt;option repeat.for="item of <xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="$foreignKey"/></xsl:call-template>s" model.bind="item.<xsl:value-of select="@foreignKeyField"/>"&gt;${item.<xsl:value-of select="../../item[@name=$foreignKey]/@uiDisplayField"/>}&lt;/option&gt;
                      &lt;/select&gt;
                    &lt;/div&gt; &lt;!-- /.form-group --&gt;
                    </xsl:when>
                    <xsl:otherwise>&lt;div class="form-group"&gt;
                      &lt;label class="control-label" for="<xsl:value-of select="text()"/>"&gt;${'<xsl:value-of select="@friendlyName"/>' |t}&lt;/label&gt;
                      <xsl:choose><xsl:when test="@type='bool'">&lt;label&gt;&lt;input type="checkbox" name="<xsl:value-of select="text()"/>" checked.bind="create.<xsl:value-of select="text()"/>" <xsl:if test="position() = 1">disabled="disabled"</xsl:if> &gt;${option.name}&lt;/label&gt;</xsl:when>
                    <xsl:otherwise>&lt;input type="text" id="<xsl:value-of select="text()"/>" name="<xsl:value-of select="text()"/>" class="form-control" value.bind="create.<xsl:value-of select="text()"/>" data-parsley-required="<xsl:value-of select="position() > 1 and @isNullable='false'"/>" autocomplete="off" <xsl:if test="position() = 1">disabled="disabled"</xsl:if> <xsl:if test="@type='int'">min="0" max="20000" step="1" data-parsley-validation-threshold="1" data-parsley-type="digits"</xsl:if>&gt;</xsl:otherwise></xsl:choose>
                    &lt;/div&gt; &lt;!-- /.form-group --&gt;
                    </xsl:otherwise></xsl:choose>
                    </xsl:for-each>

                    &lt;div class="form-group"&gt;
                      &lt;div class="text-right"&gt;
                        &lt;button type="button" class="btn btn-default" disabled.bind="submitting" data-dismiss="modal"&gt;${'Cancel' |t}&lt;/button&gt;
                        &lt;button type="submit" class="btn btn-primary" disabled.bind="submitting" &gt;${'Create <xsl:value-of select="@friendlyName"/>' |t}&lt;/button&gt;
                      &lt;/div&gt;
                    &lt;/div&gt; &lt;!-- /.form-group --&gt;
                  &lt;/form&gt;

            &lt;/div&gt; &lt;!-- /.modal-body --&gt;

          &lt;/div&gt;&lt;!-- /.modal-content --&gt;

        &lt;/div&gt;&lt;!-- /.modal-dialog --&gt;

      &lt;/div&gt; &lt;!-- /.modal --&gt;
      
      
      
      
      &lt;div id="editModal<xsl:value-of select="@name"/>" class="modal modal-styled fade" tabindex="-1" role="dialog"&gt;
        &lt;div class="modal-dialog modal-lg"&gt;
          &lt;div class="modal-content"&gt;

            &lt;div class="modal-header"&gt;
              &lt;button type="button" class="close" data-dismiss="modal" aria-hidden="true"&gt;×&lt;/button&gt;
              &lt;h3 class="modal-title" &gt;&lt;i class="fa fa-codepen icon-spaced"&gt;&lt;/i&gt;${'Edit <xsl:value-of select="@friendlyName"/>' |t}&lt;/h3&gt;
            &lt;/div&gt; &lt;!-- /.modal-header --&gt;

            &lt;div class="modal-body"&gt;
              
                  &lt;form id="editForm<xsl:value-of select="@name"/>" onsubmit="return false;" role="form" class="form parsley-form" submit.delegate="doUpdate()"  &gt;
                    <xsl:for-each select="field[not(@uiEditHidden='true')]">
                    <xsl:variable name="foreignKey"><xsl:value-of select="@foreignKey"/></xsl:variable>
                    <xsl:choose><xsl:when test="string-length(@foreignKey)>0">&lt;div class="form-group"&gt;
                      &lt;label class="control-label" for="<xsl:value-of select="text()"/>"&gt;${'<xsl:value-of select="@friendlyName"/>' |t}&lt;/label&gt;
                      &lt;select id="<xsl:value-of select="text()"/>" name="<xsl:value-of select="text()"/>" value.bind="edit.<xsl:value-of select="text()"/>" class="form-control" data-parsley-required="<xsl:value-of select="@isNullable='false'"/>" &gt;
                          &lt;option value=""&gt;${'Please Select <xsl:value-of select="@friendlyName"/>' |t}&lt;/option&gt;
                          &lt;option repeat.for="item of <xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="$foreignKey"/></xsl:call-template>s" model.bind="item.<xsl:value-of select="@foreignKeyField"/>"&gt;${item.<xsl:value-of select="../../item[@name=$foreignKey]/@uiDisplayField"/>}&lt;/option&gt;
                      &lt;/select&gt;
                    &lt;/div&gt; &lt;!-- /.form-group --&gt;
                    </xsl:when>                   
                    <xsl:otherwise>&lt;div class="form-group"&gt;
                      &lt;label class="control-label" for="<xsl:value-of select="text()"/>"&gt;${'<xsl:value-of select="@friendlyName"/>' |t}&lt;/label&gt;
                      <xsl:choose><xsl:when test="@type='bool'">&lt;label&gt;&lt;input type="checkbox" name="<xsl:value-of select="text()"/>" checked.bind="edit.<xsl:value-of select="text()"/>" <xsl:if test="position() = 1">disabled="disabled"</xsl:if> &gt;&lt;/label&gt;</xsl:when>
                      <xsl:otherwise>&lt;input type="text" id="<xsl:value-of select="text()"/>" name="<xsl:value-of select="text()"/>" class="form-control" value.bind="edit.<xsl:value-of select="text()"/>" data-parsley-required="<xsl:value-of select="@isNullable='false'"/>" autocomplete="off" <xsl:if test="position() = 1">disabled="disabled"</xsl:if> <xsl:if test="@type='int'">min="0" max="20000" step="1" data-parsley-validation-threshold="1" data-parsley-type="digits"</xsl:if>&gt;</xsl:otherwise></xsl:choose>
                    &lt;/div&gt; &lt;!-- /.form-group --&gt;
                    </xsl:otherwise></xsl:choose>
                    </xsl:for-each>

                    &lt;div class="form-group"&gt;
                      &lt;button type="button" class="btn btn-danger" disabled.bind="submitting" click.delegate="doDelete()" &gt;${'Delete' |t}&lt;/button&gt;
                        &lt;button type="submit" class="btn btn-success pull-right" disabled.bind="submitting" &gt;${'Save <xsl:value-of select="@friendlyName"/>' |t}&lt;/button&gt;
                        &lt;button type="button" class="btn btn-default pull-right btn-right-pad" disabled.bind="submitting" data-dismiss="modal"&gt;${'Cancel' |t}&lt;/button&gt;
                    &lt;/div&gt; &lt;!-- /.form-group --&gt;
                  &lt;/form&gt;

            &lt;/div&gt; &lt;!-- /.modal-body --&gt;

          &lt;/div&gt;&lt;!-- /.modal-content --&gt;

        &lt;/div&gt;&lt;!-- /.modal-dialog --&gt;

      &lt;/div&gt; &lt;!-- /.modal --&gt;
    
    
    
  &lt;/section&gt;
&lt;/template&gt;

'''[ENDFILE]

'''[STARTFILE:..\..\Generation\Output\UIHelpers\Aurelia\<xsl:value-of select="$name_lowered"/>s.js]
import {inject, computedFrom, bindable<xsl:if test="@uiAsChild='true'">, customElement</xsl:if>} from 'aurelia-framework';
import {App} from 'app';
import $ from 'jquery';
import {ui} from '../ui-helper';

<xsl:choose><xsl:when test="@uiAsChild='true'">@inject(ui, Element)</xsl:when><xsl:otherwise>@inject(App, ui)</xsl:otherwise></xsl:choose>
export class <xsl:value-of select="@name"/>s {
   @bindable data;
   @bindable create;
   @bindable refreshing;
   @bindable edit;
   @bindable submitting;
   <xsl:if test="@uiAsChild='true'">@bindable app;</xsl:if>

   <xsl:for-each select="field[not(@uiCreateHidden='true') and string-length(@foreignKey)>0]">@bindable <xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="@foreignKey"/></xsl:call-template>s;
   </xsl:for-each>

   constructor(app, ui) {
      this.app = app;
      this.ui = ui;
      this.i18n = app.i18n;
      this.create = {};
      this.pageService = this;
      this.sort = {
            skip: 0,
            step: 10,
            on: '<xsl:value-of select="@uiDefaultSort"/>',
            direction: '<xsl:choose><xsl:when test="@uiDefaultSortDescending">desc</xsl:when><xsl:otherwise>asc</xsl:otherwise></xsl:choose>',
            filter: ''
      };
   }

   activate(params, routeConfig, navigationInstruction) {
      this.resetCreateModel();
      routeConfig.navModel.title = this.i18n.tr(`<xsl:value-of select="@friendlyName"/>s`);
   }
   attached() {
      this.ui.initTooltips();
      this.ui.initParsley(this);
      
      this.$editModal = function(){ return $('#editModal<xsl:value-of select="@name"/>')};
      this.$createModal = function(){ return $('#createModal<xsl:value-of select="@name"/>')};
      this.$editForm = function(){ return $('#editForm<xsl:value-of select="@name"/>')};
      this.$createForm = function(){ return $('#createForm<xsl:value-of select="@name"/>')};
      
      this.refresh();

      <xsl:if test="count(field[not(@uiCreateHidden='true') and string-length(@foreignKey)>0])>0">this.loadDropDowns();</xsl:if>
      return true;
   }

   refresh() {
      this.sort.skip = 0;
      this.loadData();
   }
   dataSortBy(field){
      if(this.sort.on == field){
          this.sort.direction = (this.sort.direction == "asc") ? "desc" : "asc";
      } else {
          this.sort.on = field;
      }
      this.loadData();
   }
   dataStepTo(skip){ // called by pager
      this.sort.skip = skip;
      this.loadData();
   }
   dataApplySearch(searchTerm){ // called by search-bar
      this.sort.filter = searchTerm;
      this.sort.skip = 0;
      this.loadData();
   }
   loadData() {
      var self = this;
      this.refreshing = true;

      var sdk = this.app.sdk.configure("");
      sdk.<xsl:value-of select="@name"/>.Find(self.sort.skip, self.sort.step, self.sort.filter, self.sort.on, self.sort.direction=="desc")
         .done(function (data) {
            self.refreshing = false;
            self.data = data;
         })
         .error(function (data) {
            self.refreshing = false;
            self.showError(data);
         });
   }

   loadDropDowns(){
      var self = this;
      var sdk = this.app.sdk.configure("");
   <xsl:for-each select="field[not(@uiCreateHidden='true' and @uiEditHidden='true') and string-length(@foreignKey)>0]">
     <xsl:variable name="foreignKey"><xsl:value-of select="@foreignKey"/></xsl:variable>
      
      sdk.<xsl:value-of select="$foreignKey"/>.Find(0, 1000)
         .done(function (data) {
            self.<xsl:call-template name="ToLower"><xsl:with-param name="inputString" select="$foreignKey"/></xsl:call-template>s = data.items;
         })
         .error(function (data) {
            self.app.showError(self.i18n.tr(`Unable to load <xsl:value-of select="$foreignKey"/> data`));
         });
   </xsl:for-each>
   }
   
   resetCreateModel(){
      this.create = {
         
      };
   }

   showEdit(<xsl:value-of select="field[1]"/>) {
      this.edit = Object.assign({}, this.data.items.find(x =&gt; x.<xsl:value-of select="field[1]"/> == <xsl:value-of select="field[1]"/>));
   }
   hideEdit(){
      this.$editModal().modal('hide');
   }
   
   hideCreate(){
      this.$createModal().modal('hide');
   }
   
   showError(data){
      this.app.showError(data);
   }
   <xsl:for-each select="field[string-length(@priorityGroupBy)>0]">
   doUpdate<xsl:value-of select="@friendlyName"/>(item, direction, end){
      var self = this;
      var sdk = this.app.sdk.configure("");
      item.submitting = true;
      var <xsl:value-of select="text()"/> = item.<xsl:value-of select="text()"/>;
      if(end) {
            if(direction == -1){
                  <xsl:value-of select="text()"/> = 1;
            } else {
                  <xsl:value-of select="text()"/> = self.data.stepping.total;
            }
      } else {
            if(direction == -1){
                  <xsl:value-of select="text()"/> -= 1;
            } else {
                  <xsl:value-of select="text()"/> += 1;
            }
      }
      
      sdk.<xsl:value-of select="../@name"/>.Update<xsl:value-of select="../@name"/><xsl:value-of select="@friendlyName"/>Async(item.<xsl:value-of select="../field[1]"/>, <xsl:value-of select="text()"/>)
            .done(function (data) {
                  item.submitting = false;
                  self.refresh();
            })
            .error(function (data) {
                  item.submitting = false;
                  self.showError(data);
            });
   }
   </xsl:for-each>
   doCreate() {
      var self = this;

      if (self.$createForm().parsley().isValid()) {
         var sdk = this.app.sdk.configure("");
         self.submitting = true;
         
         sdk.<xsl:value-of select="@name"/>.Create<xsl:value-of select="@name"/>Async(self.create)
            .done(function (data) {
               self.submitting = false;
               self.hideCreate();
               self.resetCreateModel();
               self.refresh();
            })
            .error(function (data) {
               self.submitting = false;
               self.showError(data);
            }
         );
      }
   }

   doUpdate() {
      var self = this;

      if (this.$editForm().parsley().isValid()) {
         var sdk = this.app.sdk.configure("");
         self.submitting = true;
         
         sdk.<xsl:value-of select="@name"/>.Update<xsl:value-of select="@name"/>Async(self.edit.<xsl:value-of select="field[1]"/>, self.edit)
            .done(function (data) {
               self.submitting = false;
               self.hideEdit();
               self.refresh();
            })
            .error(function (data) {
               self.submitting = false;
               self.showError(data);
            });
      }
   }

   doDelete() {
      var self = this;

      var sdk = this.app.sdk.configure("");
         self.submitting = true;
         
         sdk.<xsl:value-of select="@name"/>.Delete<xsl:value-of select="@name"/>Async(self.edit.<xsl:value-of select="field[1]"/>)
            .done(function (data) {
               self.submitting = false;
               self.hideEdit();
               self.refresh();
            })
            .error(function (data) {
               self.submitting = false;
               self.showError(data);
            }
         );
   }

}
'''[ENDFILE]

</xsl:for-each>

</xsl:template>

	<xsl:template match="@space"> </xsl:template>
  <xsl:template name="ToLower">
          <xsl:param name="inputString"/>
          <xsl:variable name="smallCase" select="'abcdefghijklmnopqrstuvwxyz'"/>
          <xsl:variable name="upperCase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
          <xsl:value-of select="translate($inputString,$upperCase,$smallCase)"/>
  </xsl:template>
 <xsl:template name="ToUpper">
          <xsl:param name="inputString"/>
          <xsl:variable name="smallCase" select="'abcdefghijklmnopqrstuvwxyz'"/>
          <xsl:variable name="upperCase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'"/>
          <xsl:value-of select="translate($inputString,$smallCase,$upperCase)"/>
  </xsl:template>
    <xsl:template name="Pluralize">
          <xsl:param name="inputString"/>
          <xsl:choose><xsl:when test="substring($inputString, string-length($inputString)) = 'y'"><xsl:value-of select="concat(substring($inputString, 1, string-length($inputString)-1),'ies')"/></xsl:when><xsl:otherwise><xsl:value-of select="$inputString"/>s</xsl:otherwise></xsl:choose>
  </xsl:template>
</xsl:stylesheet>