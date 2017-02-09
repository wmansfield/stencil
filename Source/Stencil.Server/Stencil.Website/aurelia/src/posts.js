
import {inject, computedFrom, bindable} from 'aurelia-framework';
import {App} from 'app';
import $ from 'jquery';

@inject(App)
export class Posts {
   @bindable data;
   @bindable create;
   @bindable refreshing;
   @bindable edit;
   @bindable submitting;
   @bindable accounts;
   

   constructor(app) {
      this.app = app;
      this.create = {};
      this.pageService = this;
      this.sort = {
            skip: 0,
            step: 10,
            on: 'stamp_utc',
            direction: 'desc',
            filter: ''
      };
   }

   activate(params, routeConfig, navigationInstruction) {
      this.resetCreateModel();
   }
   attached() {
      
      this.$editModal = function(){ return $('#editModalPost')};
      this.$createModal = function(){ return $('#createModalPost')};
      this.$editForm = function(){ return $('#editFormPost')};
      this.$createForm = function(){ return $('#createFormPost')};
      
      this.refresh();

      this.loadDropDowns();
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
      sdk.Post.Find(self.sort.skip, self.sort.step, self.sort.filter, self.sort.on, self.sort.direction=="desc")
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
   
      
      sdk.Account.Find(0, 1000)
         .done(function (data) {
            self.accounts = data.items;
         })
         .error(function (data) {
            self.app.showError(`Unable to load Account data`);
         });
   
   }
   
   resetCreateModel(){
      this.create = {
         
      };
   }

   showEdit(post_id) {
      this.edit = Object.assign({}, this.data.items.find(x => x.post_id == post_id));
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
   
   doCreate() {
      var self = this;

      var sdk = this.app.sdk.configure("");
         self.submitting = true;
         
         sdk.Post.CreatePostAsync(self.create)
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

   doUpdate() {
      var self = this;

      var sdk = this.app.sdk.configure("");
         self.submitting = true;
         
         sdk.Post.UpdatePostAsync(self.edit.post_id, self.edit)
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

   doDelete() {
      var self = this;

      var sdk = this.app.sdk.configure("");
         self.submitting = true;
         
         sdk.Post.DeletePostAsync(self.edit.post_id)
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
